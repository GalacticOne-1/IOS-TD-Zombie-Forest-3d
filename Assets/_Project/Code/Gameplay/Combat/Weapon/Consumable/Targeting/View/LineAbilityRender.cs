using UnityEngine;

namespace Galactic1.Code.Gameplay.Targeting
{
    public sealed class LineAbilityRender
    {
        private readonly LineRenderer _line;
        private readonly Material _mat;

        private static readonly int ValidId  = Shader.PropertyToID("_Valid");
        private static readonly int TilingId = Shader.PropertyToID("_Tiling");

        // Сколько мировых единиц на один даш+гап цикл
        // Должно совпадать с (_DashLength + _GapLength) в шейдере
        private const float WorldUnitsPerCycle = 0.28f;

        public LineAbilityRender(LineRenderer line)
        {
            _line = line;
            _line.positionCount = 2;
            _line.startWidth = 0.06f;
            _line.endWidth   = 0.02f;
            _line.textureMode = LineTextureMode.RepeatPerSegment;
            _line.enabled = false;

            _mat = _line.material;
        }

        public void Show() => _line.enabled = true;
        public void Hide() => _line.enabled = false;

        public void Update(Vector3 from, Vector3 to, bool valid)
        {
            from.y = 0.25f;
            to.y   = 0.25f;

            _line.SetPosition(0, from);
            _line.SetPosition(1, to);

            // Считаем сколько циклов даш+гап укладывается в текущую длину
            float worldLength = Vector3.Distance(from, to);
            float tiling = worldLength / WorldUnitsPerCycle;
            _mat.SetFloat(TilingId, tiling);
            _mat.SetFloat(ValidId, valid ? 1f : 0f);
        }
    }
}