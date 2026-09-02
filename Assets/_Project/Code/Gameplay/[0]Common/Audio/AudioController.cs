using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Galactic1
{
	/*	Единичный класс CORE
	 * 	Управление всеми звуками
	 */
    public class AudioController : MonoBehaviour, IGameService, ISceneActivator
    {
	    public AudioMixer master;

	    public bool audioDisable;
	    
		public CSound[] ui;
		public CSound[] game, attack;

		// для очереди 
		List<AudioClip> queueClip = new List<AudioClip>();
		
		
		
		
		//sound battle
		/*[Space(20)]
		
		static SetAudio s1 = new SetAudio (.7f,1);
		static SetAudio s2 = new SetAudio (.5f,1.2f);
		
		SetAudio[] setA = {s1,s2};



		//-------------
		static WaitForSeconds w1 = new WaitForSeconds (.04f);
		static WaitForSeconds w2 = new WaitForSeconds (.1f);
		static WaitForSeconds w3 = new WaitForSeconds (.2f);
		WaitForSeconds[] w = {w1,w2,w3};
		//*/

		
		

		public void Activator()
		{

			
			var go = new GameObject("UI");
			go.transform.parent = transform;
			foreach (CSound s in ui)
			{
				s.source = go.AddComponent<AudioSource>();
				s.source.outputAudioMixerGroup = master.FindMatchingGroups("Game")[0];		// "UI"
				s.source.clip = s.clip;
				s.source.volume = s.volume;
				s.source.pitch = s.pitch;
				s.source.loop = s.loop;
				s.source.playOnAwake = false;
			}
			
			
			go = new GameObject("Game");
			go.transform.parent = transform;
			foreach (CSound s in game)
			{
				s.source = go.AddComponent<AudioSource>();
				s.source.outputAudioMixerGroup = master.FindMatchingGroups("Game")[0];
				s.source.clip = s.clip;
				s.source.volume = s.volume;
				s.source.pitch = s.pitch;
				s.source.loop = s.loop;
				s.source.playOnAwake = false;
			}
			
			go = new GameObject("Attack");
			go.transform.parent = transform;
			foreach (CSound s in attack)
			{
				s.source = go.AddComponent<AudioSource>();
				s.source.outputAudioMixerGroup = master.FindMatchingGroups("Game")[0];
				s.source.clip = s.clip;
				s.source.volume = s.volume;
				s.source.pitch = s.pitch;
				s.source.loop = s.loop;
				s.source.playOnAwake = false;
			}
			

			AudioListener.volume = 1f;
		}




		#region Short Sound

		/// <summary>
		/// Для кнопок и панелей
		/// </summary>
		/// <param name="type"></param>
		public void Sound_UI(int id)
		{
			if (audioDisable) return;

			ui[id].source.Play();
		}


		/// <summary>
		/// С возможностью изменения громкости и питча
		/// </summary>
		/// <param name="type"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public void Sound_Game(int id, float volume = 2, float pitch = 1)
		{
			if (audioDisable) return;

			game[id].source.volume = volume == 2 ? game[id].source.volume : volume;
			game[id].source.pitch = pitch;
			game[id].source.Play();
		}


		/// <summary>
		/// Звук запустится только если не проигрывается в данный момент
		/// <br/>Иначе ничего не будет
		/// </summary>
		/// <param name="type"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public void SoundDelay_Game(int id, float volume = 2, float pitch = 1)
		{
			if (game[id].close || audioDisable) return;

			StartCoroutine(Sound(id, volume == 2 ? game[id].source.volume : volume, pitch));
		}

		IEnumerator Sound(int id, float volume, float pitch)
		{
			game[id].close = true;

			if (game[id].clip == null) yield break;

			game[id].source.volume = volume;
			game[id].source.pitch = pitch;
			game[id].source.Play();

			yield return new WaitForSeconds(game[id].delay);
			game[id].close = false;
		}
		
		
		
		/// <summary>
		/// Звук запустится только если не проигрывается в данный момент
		/// <br/>Иначе ничего не будет
		/// </summary>
		/// <param name="type"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public void SoundDelay_Attack(int id, float volume = 2, float pitch = 1)
		{
			if (attack[id].close || audioDisable) return;

			StartCoroutine(SoundA(id, volume == 2 ? attack[id].source.volume : volume, pitch));
		}

		IEnumerator SoundA(int id, float volume, float pitch)
		{
			attack[id].close = true;

			if (attack[id].clip == null) yield break;

			attack[id].source.volume = volume;
			attack[id].source.pitch = pitch;
			attack[id].source.Play();

			yield return new WaitForSeconds(attack[id].delay);
			attack[id].close = false;
		}

		#endregion





		#region Attack

		// реализация простой очереди
		public void Sound_Queue(int id)
		{
			if (game[id].close) return;
			game[id].close = true;
			game[id].source.pitch = Random.Range(.9f, 1.1f);
			game[id].source.Play();
			StartCoroutine(queue(id));
		}
		
		IEnumerator queue(int id)
		{
			yield return new WaitForSeconds(.1f);
			game[id].close = false;
		}
		//
		

		// проигрывает звук с учетом очереди
		public bool Sound_Queue(AudioSource s)
		{
			if (audioDisable) return false;
			
			var l = queueClip.Count;
			for (int i = 0; i < l; i++)
			{
				if (queueClip[i] == s.clip)
					return false;
			}

			s.pitch = Random.Range(.9f, 1.1f);
			s.Play();
			queueClip.Add(s.clip);
			StartCoroutine(queue(s.clip));
			return true;
		}
		// когда клип отыграет, удаляем из очереди
		// делая звук снова доступным
		IEnumerator queue(AudioClip clip)
		{
			yield return new WaitForSeconds(.1f);
			queueClip.Remove(clip);
		}


		public bool Sound_Shoot(AudioSource s, bool owner)
		{
			if (audioDisable) return false;
			
			// если запрос от того кто запустил звук (step 1)
			if (owner)
			{
				s.Play();
				return true;
			}
			
			var l = queueClip.Count;
			for (int i = 0; i < l; i++)
			{
				if (queueClip[i] == s.clip)
					return false;
			}

			// STEP 1
			s.pitch = Random.Range(.9f, 1.1f);
			s.Play();
			queueClip.Add(s.clip);
			return true;
		}

		public void Sound_ShootReset(AudioSource s)
		{
			queueClip.Remove(s.clip);
		}

		#endregion
 
    
 
    




    }

    

    [System.Serializable]
    public class CSound
    {
	    public string name;
	    public AudioClip clip;
	    public float volume = .5f, pitch = 1;
	    public AudioSource source;
	    
	    public bool close,		// true - уже запущен 
		    loop;
	    public float delay;		// продолжительность звука	
    }

    public class SetAudio
    {
	    public float volume;
	    public float pitch;

	    public SetAudio(float volume, float pitch)
	    {
		    this.volume = volume;
		    this.pitch = pitch;
	    }
    }

    [System.Serializable]
    public struct CUnitSound
    {
	    public CSound[] die;
    }

}