using System.Collections;
using Galactic1;
using TMPro;
using UnityEngine;

namespace Galactic1
{
    public class StageModel : MVVMModel
    {
        public StageModel(MVVMView _view) : base(_view)
        {
            view = _view;
        }


        const byte step_position = 130;
        private float small = .75f;

        public void Load()
        {
            var vw = view as StageView;

            // WAVE
            StateWave();
            
            
            // STAGE
            Vector2 stageCoord = vw.HoldStage.GetChild(0).transform.localPosition;
            stageCoord.x = 0;
            // if (GAMEPLAY_old.stage_in_battle > 0)
            //     stageCoord.x = -step_position;
            //
            // for (int i = 0; i < 5; i++)
            // {
            //     // transparency
            //     if(GAMEPLAY_old.stage_in_battle == 0 && i > 1)
            //         vw.HoldStage.GetChild(i).GetComponent<CanvasGroup>().alpha = 0;
            //     else if (GAMEPLAY_old.stage_in_battle >= 1 && i > 2)
            //         vw.HoldStage.GetChild(i).GetComponent<CanvasGroup>().alpha = 0;
            //     
            //     // size
            //     vw.HoldStage.GetChild(i).transform.localScale = Vector2.one * (GAMEPLAY_old.stage_in_battle == 0
            //         ? i == 0 ? 1 : small
            //         : i == 1 ? 1 : small);
            //     
            //     // coord
            //     vw.HoldStage.GetChild(i, 0).GetComponent<TextMeshProUGUI>().text =
            //         GAMEPLAY_old.stage_in_battle > 0
            //             ? $"{GAMEPLAY_old.stage_in_battle + i}"
            //             : $"{i + 1}";
            //
            //     
            //     vw.HoldStage.GetChild(i).transform.localPosition = stageCoord;
            //     stageCoord.x += step_position;
            // }
        }

        
        /// <summary>
        /// Отображение волны
        /// </summary>
        public void StateWave()
        {
            var vw = view as StageView;
            // var s = $"{GAMEPLAY_old.WaveInStage()+1}/{AppConstants.wave_in_stage}";
            // s = s.SetText(35);
            // vw.TWave.text = $"Wave \n{s}";
            // vw.FillWave.fillAmount = 1;
        }

        
        /// <summary>
        /// Отображение этапов
        /// </summary>
        public void NextStage()
        {
            var vw = view as StageView;

            vw.StartCoroutine(move());
        }

        IEnumerator move()
        {
            var vw = view as StageView;
            Vector2 coord;
            float time = 0;

            CanvasGroup el_old, el_new ;
            Transform tr_old, tr_new;
            // if (GAMEPLAY_old.stage_in_battle >= 2)
            // {
            //     el_old = vw.HoldStage.GetChild(0).GetComponent<CanvasGroup>();
            //     el_new = vw.HoldStage.GetChild(3).GetComponent<CanvasGroup>();
            //     tr_old = vw.HoldStage.GetChild(1).transform;
            //     tr_new = vw.HoldStage.GetChild(2).transform;
            // }
            // else
            // {
            //     el_old = null;
            //     el_new = vw.HoldStage.GetChild(2).GetComponent<CanvasGroup>();
            //     tr_old = vw.HoldStage.GetChild(0).transform;
            //     tr_new = vw.HoldStage.GetChild(1).transform;
            // }
            

            // устанавливаем новую позицию для каждого элемента
            float[] target = new float[5];
            for (int i = 0; i < 5; i++)
            {
                target[i] = vw.HoldStage.GetChild(i).transform.localPosition.x;
                target[i] -= step_position;
            }

            // двигаем
            while (time < vw.DurationMovement)
            {
                // transparency
                // скрываем первый и показываем первый скрытый
                // if (GAMEPLAY_old.stage_in_battle >= 2)
                //     el_old.alpha = Mathf.Lerp(el_old.alpha, 0, time / vw.DurationMovement);
                // if (GAMEPLAY_old.stage_in_battle < 9)
                //     el_new.alpha = Mathf.Lerp(el_new.alpha, 1, time / vw.DurationMovement);
                //
                // // size
                // tr_old.localScale = Vector2.one * Mathf.Lerp(tr_old.localScale.x, small, time / vw.DurationMovement);
                // tr_new.localScale = Vector2.one * Mathf.Lerp(tr_new.localScale.x, 1, time / vw.DurationMovement);
                //
                // // mpvement
                // for (int i = 0; i < 5; i++)
                // {
                //     coord = vw.HoldStage.GetChild(i).transform.localPosition;
                //     coord.x = Mathf.Lerp(coord.x, target[i], time / vw.DurationMovement);
                //     vw.HoldStage.GetChild(i).transform.localPosition = coord;
                // }
                // time += Time.deltaTime;
                yield return null;
            }

            // первый элемент ставим в конец
            // if (GAMEPLAY_old.stage_in_battle >= 2)
            // {
            //     coord = vw.HoldStage.GetChild(4).transform.localPosition;
            //     coord.x += step_position;
            //     vw.HoldStage.GetChild(0).transform.localPosition = coord;
            //     vw.HoldStage.GetChild(0, 0).GetComponent<TextMeshProUGUI>().text = $"{GAMEPLAY_old.stage_in_battle + 4}";
            //     vw.HoldStage.GetChild(0).transform.SetSiblingIndex(4);
            // }

            DLog.Alert(">>> Stage movement complete!", EDlogColor.YELLOW);
        }
    }
}