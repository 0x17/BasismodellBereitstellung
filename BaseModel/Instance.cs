namespace BasismodellBereitstellung.BaseModel
{
    public class Instance
    {
        public Asset[] Assets;
        public Component[] Components;
        public int[] Periods;

        public Instance(Asset[] assets, Component[] components, int[] periods)
        {
            Assets = assets;
            Components = components;
            Periods = periods;
        }
    }
}