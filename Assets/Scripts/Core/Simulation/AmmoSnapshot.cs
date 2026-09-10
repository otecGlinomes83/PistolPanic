namespace PistolPanic.Core
{
    public readonly struct AmmoSnapshot
    {
        public AmmoSnapshot(int magazine, int magazineSize, float regenProgress01, bool isFullReloading, float reloadProgress01)
        {
            Magazine = magazine;
            MagazineSize = magazineSize;
            RegenProgress01 = regenProgress01;
            IsFullReloading = isFullReloading;
            ReloadProgress01 = reloadProgress01;
        }

        public int Magazine { get; }

        public int MagazineSize { get; }

        public float RegenProgress01 { get; }

        public bool IsFullReloading { get; }

        public float ReloadProgress01 { get; }
    }
}
