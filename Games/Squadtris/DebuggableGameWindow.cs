using ComponentKit;
using ComponentKit.Model;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Squadtris {
    internal class DebuggableGameWindow : GameWindow {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr zeroOnly, string lpWindowName);

        IntPtr _windowHandle;
        IntPtr _consoleHandle;

        bool _isShowingConsole = false;

        IEntityRecord _selectedEntity;

        const string SelectCommand = "select";
        const string SelectCommandShorthand = "sel";

        const string KillCommand = "kill";

        public DebuggableGameWindow(int width, int height, string title)
            : base(width, height, GraphicsMode.Default, title) {
            Console.Title = "Squadtris (debug)";
            Console.SetWindowSize(80, 40);

            _windowHandle = FindWindowByCaption(IntPtr.Zero, Title);
            _consoleHandle = FindWindowByCaption(IntPtr.Zero, Console.Title);
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            Console.WriteLine(GetTimestampedMessage("press ~ to toggle console"));
            Console.WriteLine(GetTimestampedMessage("..."));
        }

        protected void ToggleConsole() {
            _isShowingConsole = !_isShowingConsole;

            if (SetForegroundWindow(
                _isShowingConsole ?
                    _consoleHandle :
                    _windowHandle)) {
                if (_isShowingConsole) {
                    BeginParsingCommand();
                }
            }
        }

        void BeginParsingCommand() {
            ConsoleColor previousForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(GetTimestampedMessage("<- "));

            ParseCommand(Console.ReadLine());

            Console.ForegroundColor = previousForegroundColor;
        }

        void Select(string entityName) {
            if (!string.IsNullOrEmpty(entityName)) {
                _selectedEntity = Entity.Find(entityName);

                if (_selectedEntity != null) {
                    WriteInfo(String.Format("-> selected: {0}",
                        _selectedEntity.ToString()));
                } else {
                    WriteWarning("-> that entity does not exist");
                }
            }
        }

        void Kill(string entityName) {
            if (!string.IsNullOrEmpty(entityName)) {
                if (_selectedEntity != null && _selectedEntity.Name.Equals(entityName)) {
                    _selectedEntity = null;
                }

                if (Entity.Drop(entityName)) {
                    WriteInfo("-> killed");
                } else {
                    WriteWarning("-> that entity was not killed");
                }
            }
        }

        void ParseCommand(string command) {
            string[] arguments = command.Split(' ');

            if (arguments.Length > 1) {
                if (arguments[0].Equals(SelectCommandShorthand) ||
                    arguments[0].Equals(SelectCommand)) {
                    Select(entityName: arguments[1]);
                } else if (arguments[0].Equals(KillCommand)) {
                    Kill(entityName: arguments[1]);
                }
            } else {
                WriteWarning("-> this did nothing");
            }

            ToggleConsole();
        }

        string GetTimestampedMessage(string message) {
            return String.Format("{0}: {1}",
                DateTime.Now.ToLongTimeString(),
                message);
        }

        protected void WriteWarning(string message) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(GetTimestampedMessage(message));
        }

        protected void WriteInfo(string message) {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(GetTimestampedMessage(message));
        }
    }
}
