

namespace Galactic1
{
    public abstract class AssetConstructScr
    {
        public abstract void Init();

        public abstract void NewSaveData();


        public abstract sbyte GetMark();
        public abstract BuildableConfig[] GetList();
        public abstract void Unlock(int i);



        public abstract byte GetUsedAmount();

    }

    public class AssetConstruct_0 : AssetConstructScr
    {
        public override void Init()
        {
            var list = GetList();
            var l = list.Length;
            for (int i = 0; i < l; i++)
            {
                //list[i].SetId(i);
            }
        }

        public override void NewSaveData()
        {
            // var list = GetList();
            // var l = list.Length;
            // GAMEPLAY.DataGameplay().construct_0 = new byte[l];
            // for (int i = 0; i < l; i++)
            // {
            //     GAMEPLAY.DataGameplay().construct_0[i] =
            //         list[i].requiresBlueprint == 0
            //             ? (byte)EAssetState.USING
            //             : (byte)EAssetState.LOCK;
            // }
        }

        public override sbyte GetMark() => 0;

        public override BuildableConfig[] GetList()
            => null;// ServiceLocator.Current.Get<LibController>().furnituresSettings.GetList_0();

        public override void Unlock(int i)
        {
            // if ((EAssetState)GAMEPLAY.DataGameplay().construct_0[i] == EAssetState.LOCK)
            // {
            //     GAMEPLAY.DataGameplay().construct_0[i] = (byte)EAssetState.USING;
            // }
        }

        public override byte GetUsedAmount() => 0;
    }
    
    public class AssetConstruct_1 : AssetConstructScr
    {
        public override void Init()
        {
            var list = GetList();
            var l = list.Length;
            for (int i = 0; i < l; i++)
            {
                //list[i].SetId(i);
            }
        }

        public override void NewSaveData(){}

        public override sbyte GetMark() => 1;

        public override BuildableConfig[] GetList()
            => null;//ServiceLocator.Current.Get<LibController>().furnituresSettings.GetList_1();

        public override void Unlock(int i) {}

        public override byte GetUsedAmount() => 0;
    }
    
}