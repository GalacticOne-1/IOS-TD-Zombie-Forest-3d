using UnityEngine;

namespace Galactic1.AbstractFactory
{
    public class _UnitInterface : MonoBehaviour
    {
        
        public enum EGameSide
        {
            Player = 1, Enemy = 2
        }
        
        
        [SerializeField] private EGameSide team;

        private bool showLogs;
        private bool onlyThisLogs;

        [Space] 
        [SerializeField] private bool requestLogic;
        
        [Space] 
        [SerializeField] private bool fullAttack;           // цель всегда будет получать урон, даже если вышла из радиуса
        [SerializeField] private bool freezeVisual; 
        [Space]
        [SerializeField] private CState requestState;       // какие состояния нужны юниту

        


        public EGameSide Team => team;

        public bool ShowLogs => showLogs;
        public bool OnlyThisLogs => onlyThisLogs;

        public bool RequestLogic => requestLogic;
        public bool FullAttack => fullAttack;

        public bool FreezeVisual => freezeVisual;

        public CState RequestState => requestState;

        

        

        [System.Serializable] 
        public struct CState
        {
            public bool idle;
            public bool movement;
            public bool chase;
            public bool attack;
            public bool die;
        }



        #region EDITOR

        public void _ShowLogs(bool y) => showLogs = y;
        public void _OnlyThisLogs(bool y) => onlyThisLogs = y;

        
        public void _Team(EGameSide team) => this.team = team;

        public void _Logic(bool y) => requestLogic = y;

        public void _FullAttack(bool y) => fullAttack = y;
        public void _FreezeVisual(bool y) => freezeVisual = y;

        public void _State(CState requestState) => this.requestState = requestState;


        #endregion

    }
}