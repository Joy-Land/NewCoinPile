namespace ThinRL.Core
{
    public interface IManager
    {
        void Init();
        void Update(uint currentFrameCount);
        void OnPause();
        void OnResume();
        void OnDestroy();
    }
}