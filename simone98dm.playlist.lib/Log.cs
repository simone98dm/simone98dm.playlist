namespace simone98dm.playlist.lib
{
    public static class Log
    {
        public static void Info(string text)
        {
            Print(ConsoleColor.White, text);
        }

        public static void Error(string text)
        {
            Print(ConsoleColor.Red, $"[x] {text}");
        }

        public static void Success(string text)
        {
            Print(ConsoleColor.Green, $"[+] {text}");
        }

        public static void Warning(string text)
        {
            Print(ConsoleColor.Yellow, $"[!] {text}");
        }

        private static void Print(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
