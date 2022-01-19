using System;

namespace MenuSystem
{

    public class MenuItem
    {
        public virtual string MenuTitle { get; set; }
        public virtual string Label { get; set; }
        public virtual string MenuLetter { get; set; }

        public virtual Action MethodToExecute { get; set; }

        public MenuItem(string menuTitle, string label, string menuLetter, Action methodToExecute)
        {
            MenuTitle = menuTitle.Trim();
            Label = label.Trim();
            MenuLetter = menuLetter.Trim();
            MethodToExecute = methodToExecute;
        }

        public override string ToString()
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(MenuTitle);
            Console.ResetColor();
            return MenuLetter + ") " + Label;
            
        }
    }
}