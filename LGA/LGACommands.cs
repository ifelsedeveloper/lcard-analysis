using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LGA
{
    public static class LGACommands
    {
        static RoutedUICommand exit = new RoutedUICommand("Выход", "Exit", typeof(LGACommands));

        public static RoutedUICommand Exit { get { return exit; } }

        static RoutedUICommand about = new RoutedUICommand("О Программе", "About", typeof(LGACommands));

        public static RoutedUICommand About { get { return about; } } 
    }
}
