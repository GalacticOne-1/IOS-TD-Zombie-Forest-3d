using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class DetailObjModel : MVVMModel
    {

       
        

        public DetailObjModel(MVVMView _view) : base(_view)
        {
            view = _view;
        }


        
        
        /// <summary>
        /// Запускает панель оповещения с закрытием через кнопкку и блокировкой экрана
        /// </summary>
        public void Show(string title, string des, _EntityConfig_.CStatGUI[] stat)
        {
            var vw = view as DetailObjView;
            
            vw.gameObject.GetChild(0).EventBtnOne_old(() => vw.Hide());

            vw.TTitle.text = title;
            vw.TDes.text = des;

            var l = vw.CStat.transform.childCount;
            for (int i = 0; i < l; i++)
            {
                if (i >= stat.Length)
                {
                    vw.CStat.GetChild(i).SetActive(false);
                    continue;
                }
                
                vw.CStat.GetChild(i).SetActive(true);
                vw.CStat.GetChild(i, 0).GetComponent<TextMeshProUGUI>().text = stat[i].title;
                vw.CStat.GetChild(i, 1).GetComponent<TextMeshProUGUI>().text = stat[i].value;
                vw.CStat.GetChild(i, 2).GetComponent<Image>().sprite = stat[i].icon;
            }
        }
        
    }
}