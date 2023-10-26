using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collapse
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            using (CollapseGame game = new CollapseGame())
            {
                game.Run();
            }
        } 
    }
}
