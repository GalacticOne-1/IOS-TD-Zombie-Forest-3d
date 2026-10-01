using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Analytics
{
    /// <summary>
    /// Root-lifetime сервис аналитики. Не знает про Firebase и про gameplay.
    ///
    /// Поведение:
    ///  - Idle/Initializing: события идут в буфер (32, при переполнении вытесняется самое старое).
    ///  - Ready: события уходят провайдеру сразу; буфер сбрасывается при переходе в Ready.
    ///  - Failed: события отбрасываются. Start() можно вызвать снова (retry делает вызывающий).
    ///  - Disabled (SetEnabled(false)): события отбрасываются, буфер очищается, НЕ копится.
    ///  - Любое исключение провайдера перехватывается; 3 подряд -> Failed. Игра продолжается.
    /// Только main thread.
    /// </summary>
    public sealed class GameAnalyticsService : IAnalyticsService
    {
        private enum State { Idle, Initializing, Ready, Failed }

        private const int BufferCapacity = 32;
        private const int MaxConsecutiveErrors = 3;

        private readonly IAnalyticsProvider _provider;
        private readonly int _schemaVersion;
        private readonly string _environment;

        private readonly Queue<AnalyticsEvent> _buffer = new(BufferCapacity);
        private readonly Dictionary<string, string> _pendingUserProperties = new();

        private State _state = State.Idle;
        private bool _enabled = true;
        private int _suppressDepth;
        private int _consecutiveErrors;

        public bool IsReady => _state == State.Ready;
        public bool IsEnabled => _enabled;

        public GameAnalyticsService(IAnalyticsProvider provider, int schemaVersion, string environment)
        {
            _provider = provider;
            _schemaVersion = schemaVersion;
            _environment = environment;
        }

        // =========================================================
        // LIFECYCLE
        // =========================================================

        /// <summary>Запускает инициализацию провайдера. Не блокирует. Безопасно вызывать повторно:
        /// игнорируется в Initializing/Ready, в Failed запускает новую попытку.</summary>
        public void Start()
        {
            if (_state == State.Initializing || _state == State.Ready)
                return;

            _state = State.Initializing;

            try
            {
                _provider.Initialize(OnProviderInitialized);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Analytics] Provider initialize threw: {ex.Message}");
                _state = State.Failed;
                _buffer.Clear();
            }
        }

        private void OnProviderInitialized(bool success)
        {
            if (!success)
            {
                Debug.LogWarning("[Analytics] Provider unavailable, analytics disabled for this attempt.");
                _state = State.Failed;
                _buffer.Clear();
                return;
            }

            _state = State.Ready;
            _consecutiveErrors = 0;

            // Всегда пробрасываем текущее состояние гейта (провайдер мог стартовать выключенным).
            Safe(() => _provider.SetCollectionEnabled(_enabled));

            if (!_enabled)
            {
                _buffer.Clear();
                _pendingUserProperties.Clear();
                return;
            }

            foreach (var kv in _pendingUserProperties)
            {
                var name = kv.Key;
                var value = kv.Value;
                Safe(() => _provider.SetUserProperty(name, value));
            }
            _pendingUserProperties.Clear();

            while (_buffer.Count > 0 && _state == State.Ready)
                Send(_buffer.Dequeue());
        }

        // =========================================================
        // PUBLIC API
        // =========================================================

        public void LogEvent(AnalyticsEvent analyticsEvent)
        {
            if (analyticsEvent == null || _suppressDepth > 0 || !_enabled)
                return;

            Decorate(analyticsEvent);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!AnalyticsValidator.IsValid(analyticsEvent, out var error))
            {
                Debug.LogError($"[Analytics] Invalid event dropped: {error}");
                return;
            }
#endif

            switch (_state)
            {
                case State.Ready:
                    Send(analyticsEvent);
                    break;

                case State.Idle:
                case State.Initializing:
                    Enqueue(analyticsEvent);
                    break;

                // Failed: отбрасываем.
            }
        }

        public void SetUserProperty(string name, string value)
        {
            if (!_enabled)
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!AnalyticsValidator.IsValidUserProperty(name, value, out var error))
            {
                Debug.LogError($"[Analytics] Invalid user property dropped: {error}");
                return;
            }
#endif

            switch (_state)
            {
                case State.Ready:
                    Safe(() => _provider.SetUserProperty(name, value));
                    break;

                case State.Idle:
                case State.Initializing:
                    _pendingUserProperties[name] = value; // last-write-wins
                    break;
            }
        }

        public void SetEnabled(bool enabled)
        {
            if (_enabled == enabled)
                return;

            _enabled = enabled;

            if (!enabled)
            {
                _buffer.Clear();
                _pendingUserProperties.Clear();
            }

            if (_state == State.Ready)
                Safe(() => _provider.SetCollectionEnabled(enabled));
        }

        public IDisposable Suppress()
        {
            _suppressDepth++;
            return new SuppressionToken(this);
        }

        // =========================================================
        // INTERNAL
        // =========================================================

        private void Decorate(AnalyticsEvent e)
        {
            e.Params.Add(AnalyticsParam.Of(AnalyticsParams.SchemaVersion, (long)_schemaVersion));
            e.Params.Add(AnalyticsParam.Of(AnalyticsParams.Environment, _environment));
        }

        private void Enqueue(AnalyticsEvent e)
        {
            if (_buffer.Count >= BufferCapacity)
                _buffer.Dequeue(); // вытесняем самое старое

            _buffer.Enqueue(e);
        }

        private void Send(AnalyticsEvent e)
        {
            Safe(() => _provider.LogEvent(e));
        }

        private void Safe(Action action)
        {
            try
            {
                action();
                _consecutiveErrors = 0;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Analytics] Provider error: {ex.Message}");

                if (++_consecutiveErrors >= MaxConsecutiveErrors)
                {
                    Debug.LogWarning("[Analytics] Too many provider errors, analytics switched to Failed.");
                    _state = State.Failed;
                    _buffer.Clear();
                }
            }
        }

        private sealed class SuppressionToken : IDisposable
        {
            private GameAnalyticsService _owner;

            public SuppressionToken(GameAnalyticsService owner) { _owner = owner; }

            public void Dispose()
            {
                if (_owner == null) return;
                _owner._suppressDepth--;
                _owner = null;
            }
        }
    }
}