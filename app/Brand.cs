namespace StadiaStudio {
    public static class Brand {
        public const string Owner="bernhardsf";
        public const string Product="StadiaLink";
        public const string Mark="local";
        public static string Title {get{return Product+" ("+Mark+")";}}
        public static string Stamp {get{return Product+" "+Mark+" · "+Owner;}}
        public static string About {get{return "1.0.4 · GPLv3 · local build by "+Owner+"";}}
    }
}
