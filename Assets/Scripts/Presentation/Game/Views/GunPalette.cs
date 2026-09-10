namespace PistolPanic.Presentation
{
    public readonly struct GunPalette
    {
        public readonly uint Body;

        public readonly uint BodyLight;

        public readonly uint BodyDark;

        public readonly uint Accent;

        public readonly uint AccentLight;

        public readonly uint AccentDark;

        public readonly uint Wood;

        public readonly uint WoodLight;

        public readonly uint WoodDark;

        public GunPalette(uint body, uint bodyLight, uint bodyDark, uint accent, uint accentLight, uint accentDark, uint wood, uint woodLight, uint woodDark)
        {
            Body = body;
            BodyLight = bodyLight;
            BodyDark = bodyDark;
            Accent = accent;
            AccentLight = accentLight;
            AccentDark = accentDark;
            Wood = wood;
            WoodLight = woodLight;
            WoodDark = woodDark;
        }
    }
}
