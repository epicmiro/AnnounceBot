using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnounceBot
{
    class Program
    {
        private static AnnouceBot _bot;
        static void Main(string[] args)
        {
            _bot = new AnnouceBot();
            _bot.Start();
        }
    }
}
