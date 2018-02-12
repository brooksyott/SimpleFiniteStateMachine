using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneCallTest
{
    class Program
    {
        static void Main(string[] args)
        {
            PhoneCall phone = new PhoneCall("A Party");

            phone.TakeOffHook();
            phone.Dialed("B Party");
            phone.Connected();
            phone.SetVolume(8);
            phone.Mute();
            phone.Mute();
            phone.Unmute();
            phone.HangUp();
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
