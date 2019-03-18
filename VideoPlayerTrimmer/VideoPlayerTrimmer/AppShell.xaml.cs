﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VideoPlayerTrimmer
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AppShell : Xamarin.Forms.Shell
    {
		public AppShell ()
		{
			InitializeComponent();
            Route = App.NavRoute;
            RouteHost = App.RouteHost;
            RouteScheme = App.RouteHost;
		}
	}
}