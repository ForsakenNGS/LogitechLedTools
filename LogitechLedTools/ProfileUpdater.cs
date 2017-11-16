using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogitechLedTools
{
    class ProfileUpdater
    {

        private int hwnd;
        private IDictionary<string,ScreenChecker> checks;

        public ProfileUpdater(int hwndTarget)
        {
            hwnd = hwndTarget;
            checks = new Dictionary<string, ScreenChecker>();
        }

        public ScreenChecker CreateCheck(string name)
        {
            checks.Add(name, new ScreenChecker(hwnd, null));
            return checks[name];
        }

        public ScreenChecker CreateCheck(string name, string scene)
        {
            checks.Add(name, new ScreenChecker(hwnd, scene));
            return checks[name];
        }

        public void Execute()
        {
            Execute(null);
        }

        public void Execute(string scene)
        {
            foreach (var checkEntry in checks)
            {
                var check = checkEntry.Value;
                if (scene == check.scene)
                {
                    checkEntry.Value.Execute();
                }
            }
        }

        public ScreenChecker GetCheck(string name)
        {
            if (checks.ContainsKey(name))
            {
                return checks[name];
            }
            return null;
        }

        public bool GetPointResult(string checkName, string pointName)
        {
            if (checks.ContainsKey(checkName))
            {
                return checks[checkName].GetPointResult(pointName);
            }
            return false;
        }

        public double GetPointPercentage(string checkName)
        {
            if (checks.ContainsKey(checkName))
            {
                return Convert.ToDouble(checks[checkName].countSuccessful) / Convert.ToDouble(checks[checkName].countDone);
            }
            return 0;
        }

        public double GetBarResult(string checkName, string barName)
        {
            if (checks.ContainsKey(checkName))
            {
                return checks[checkName].GetBarResult(barName);
            }
            return 0;
        }

    }
}
