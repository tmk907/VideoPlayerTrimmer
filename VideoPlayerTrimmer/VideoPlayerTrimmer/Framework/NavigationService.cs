using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace VideoPlayerTrimmer.Framework
{
    public class NavigationService
    {
        public NavigationService()
        {
            Shell.Current.Navigating += Shell_Navigating;
        }

        private void Shell_Navigating(object sender, ShellNavigatingEventArgs e)
        {
            App.DebugLog(string.Format("OnNavigating Source: {0} Target: {1} Current:{2}", e.Source.ToString(), e.Target?.Location.ToString(), e.Current.Location));

            NavigationTargetLocation = e.Target?.Location;
        }

        public Uri NavigationTargetLocation { get; private set; }

        public Dictionary<string, string> GetNavigationParameters()
        {
            return ParseNavigationParameters(NavigationTargetLocation?.ToString());
        }

        public string BackNavigationParameters { get; set; }

        public Dictionary<string,string> ParseNavigationParameters(string query)
        {
            var lookupDict = new Dictionary<string, string>();
            if (query == null)
                return lookupDict;

            query = query.Split('?').Last();

            foreach (var part in query.Split('&'))
            {
                var parameters = part.Split('=');
                if (parameters.Length != 2)
                    continue;
                lookupDict[parameters[0]] = parameters[1];
            }

            return lookupDict;
        }

        public Task NavigateToAsync(string pageName, bool animate = false)
        {
            return Shell.Current.GoToAsync(pageName, animate);
        }

        public Task NavigateBackAsync(bool animated = false)
        {
            return Shell.Current.Navigation.PopAsync(animated);
        }

        static Dictionary<string, string> ParseQueryString(string query)
        {
            if (query.StartsWith("?", StringComparison.Ordinal))
                query = query.Substring(1);
            Dictionary<string, string> lookupDict = new Dictionary<string, string>();
            if (query == null)
                return lookupDict;
            foreach (var part in query.Split('&'))
            {
                var p = part.Split('=');
                if (p.Length != 2)
                    continue;
                lookupDict[p[0]] = p[1];
            }

            return lookupDict;
        }
    }
}
