using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1
{
    // управление музыкой в игре
    // плавная смена треков, запуск в случайном порядке или поочереди
    public class MusicManagement : MonoBehaviour, IGameService, ISceneActivator
    {
        [SerializeField] private AudioSource introLoop, menuLoop;
        public AudioClip[] clipGameplay;
        public bool playMusic;
        
        [Space(10)]
        [Header("Запускает треки в случайном порядке")]
        public bool randomTraks;
        
        /*
         *    Проигранный трек попадает в список и не учавствует в выборе
         *    до тех пор пока список не превысит установленное число playedAmount
         */
        [Header("Кол-во проигранных треков до сброса"), Range(0, 10)] 
        public byte playedAmount;
        //[Header("Проигранные треки, для исключения повторения")]
        List<byte> playedTracks = new List<byte>();


        private int currTrack = -1;



        [Space(10)] 
        private AudioSource[] sourceST = new AudioSource[2];
        bool activeMusicSource;
        IEnumerator musicTransition;


        private float waitNextTrack;
        private bool stopProcess = true;
        private int timeFade = 4;     // sec
        
        
        
        
        
        
        public void Activator()
        {
			
            sourceST[0] = gameObject.AddComponent<AudioSource>();
            sourceST[1] = gameObject.AddComponent<AudioSource>();
            
            if(introLoop)
            introLoop.outputAudioMixerGroup = ServiceLocator.Current.Get<AudioController>().master.FindMatchingGroups("Music")[0];
            
            sourceST[0].outputAudioMixerGroup =
                sourceST[1].outputAudioMixerGroup = ServiceLocator.Current.Get<AudioController>().master.FindMatchingGroups("Music")[0];
        }


        
        
        
        private void Update()
        {
            // if(Input.GetKeyDown(KeyCode.P))
            //     sourceST[0].Play();
            //
            // if (Input.GetKeyDown(KeyCode.S))
            //     LobbyStop();

            /*if (Input.GetKeyDown(KeyCode.N))
            {
                stopProcess = true;
                GameplayNextTrack(false);
            }*/

            
            // таймер до смены трека
            if (!stopProcess)
            {
                waitNextTrack -= Time.deltaTime;
                if (waitNextTrack < 0)
                {
                    stopProcess = true;
                    //if (SceneManagement.I.level)
                        GameplayNextTrack(false);
                    //else
                        //LobbyNextTrack();
                }
            }
        }


        // запуск при смене сцены
        public void Lobby()
        {
            MusicLobby();            // для игры castle hero !!!!
            
            if (playMusic)
                introLoop.Play();
        }

        // завершение музыки лобби  (когда меняется сцена)
        public void LobbyStop()
        {
            stopProcess = true;
            if (musicTransition != null)
                StopCoroutine(musicTransition);
            StartCoroutine(fade(introLoop));
        }


        // запуск при смене сцены
        public void Gameplay()
        {
            if (!playMusic) return;
            activeMusicSource = true;
            GameplayNextTrack(true);
            stopProcess = false;
        }
        void GameplayNextTrack(bool newGame)
        {
            // определяем следующий трек

            AudioClip track = null;
            if (randomTraks)
            {
                List<byte> freeTracks = new List<byte>();
                var l = clipGameplay.Length;
                var e = playedTracks.Count;
                for (byte i = 0; i < l; i++)
                {
                    bool y = true;
                    for (byte j = 0; j < e; j++)
                    {
                        if (i == playedTracks[j])
                        {
                            y = false;
                            break;
                        }
                    }

                    // добавляем непроигранные треки
                    if (y)
                        freeTracks.Add(i);
                }

                // выбираем трек
                var r = Random.Range(0, freeTracks.Count);
                playedTracks.Add(freeTracks[r]);
                if (playedTracks.Count > playedAmount)
                {
                    playedTracks.RemoveAt(0);
                }

                //waitNextTrack = clipGameplay[freeTracks[r]].length-5;
                //waitNextTrack -= timeFade;
                waitNextTrack = clipGameplay[freeTracks[r]].length + 5;
                
                track = clipGameplay[freeTracks[r]];
            }
            
            else
            {
                currTrack = currTrack < clipGameplay.Length - 1 ? currTrack + 1 : 0;
                //waitNextTrack = clipGameplay[currTrack].length-5;
                //waitNextTrack -= timeFade;
                waitNextTrack = clipGameplay[currTrack].length + 5;
                
                track = clipGameplay[currTrack];
            }
            
            
            // запуск затухания текущего трека
            if (newGame)
            {
                sourceST[0].clip = track;
                sourceST[0].Play();
            }
            else 
                NewTrackNoFade(track);
            
        }
        
        // завершение музыки игры (когда меняется сцена)
        public void GameplayStop()
        {
            stopProcess = true;
            if (musicTransition != null)
                StopCoroutine(musicTransition);
            StartCoroutine(fade(sourceST[0]));
            StartCoroutine(fade(sourceST[1]));
        }



        // затухание трека
        IEnumerator fade(AudioSource aus)
        {
            float f = 30;
            for (int i = 0; i < f; i++)
            {
                aus.volume = (f - i) * (1f / f);
                
                yield return null;
            }
            
            aus.Stop();
            aus.volume = 1;
        }
        

        // без затухания
        public void NewTrackNoFade(AudioClip clip)
        {

            int nextSource = !activeMusicSource ? 0 : 1;
            int currentSource = activeMusicSource ? 0 : 1;

            //If the clip is already being played on the current audio source, we will end now and prevent the transition
            if (clip == sourceST[currentSource].clip)
                return;

            //If a transition is already happening, we stop it here to prevent our new Coroutine from competing
            if (musicTransition != null)
                StopCoroutine(musicTransition);

            sourceST[currentSource].Stop();
            sourceST[nextSource].clip = clip;
            sourceST[nextSource].volume = 1;
            sourceST[nextSource].Play();
            
            activeMusicSource = !activeMusicSource;
            stopProcess = false;
        }
        
        
        
        // играет с затуханием между треками
        public void NewTrack(AudioClip clip)
        {

            int nextSource = !activeMusicSource ? 0 : 1;
            int currentSource = activeMusicSource ? 0 : 1;

            //If the clip is already being played on the current audio source, we will end now and prevent the transition
            if (clip == sourceST[currentSource].clip)
                return;

            //If a transition is already happening, we stop it here to prevent our new Coroutine from competing
            if (musicTransition != null)
                StopCoroutine(musicTransition);

            sourceST[nextSource].clip = clip;
            sourceST[nextSource].Play();

            musicTransition = transition(timeFade * 10);
            StartCoroutine(musicTransition);
        }
        IEnumerator transition(int transitionDuration)
        {

            for (int i = 0; i < transitionDuration + 1; i++)
            {
                sourceST[0].volume = activeMusicSource
                    ? (transitionDuration - i) * (1f / transitionDuration)
                    : (0 + i) * (1f / transitionDuration);
                sourceST[1].volume = !activeMusicSource
                    ? (transitionDuration - i) * (1f / transitionDuration)
                    : (0 + i) * (1f / transitionDuration);


                //------------------------------------------------------------//

                yield return new WaitForSecondsRealtime(0.1f);
                //use realtime otherwise if you pause the game you could pause the transition half way
            }

            //finish by stopping the audio clip on the now silent audio source
            sourceST[activeMusicSource ? 0 : 1].Stop();

            activeMusicSource = !activeMusicSource;
            musicTransition = null;

            stopProcess = false;
        }




        #region Hero Castle

        public void MusicLobby()
        {
            introLoop.clip = clipGameplay[0];
            introLoop.volume = 1;
            introLoop.Play();
        }

        public void MusicBattle()
        {
            introLoop.clip = clipGameplay[1];
            introLoop.volume = .2f;
            introLoop.Play();
        }

        public void MusicStop() => introLoop.Stop();

        #endregion

    }
}