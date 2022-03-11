// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ManagedCommon;
using Wox.Plugin;

namespace ExamplePlugin
{
    public class Main : IPlugin, IPluginI18n, IContextMenu, IDisposable
    {
        public string Name => Properties.Resources.plugin_name;

        public string Description => Properties.Resources.plugin_description;

        private PluginInitContext _context;
        private static string _icon_path;
        private bool _disposed;

        public void Init(PluginInitContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(paramName: nameof(context));
            }

            _context = context;
            _context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(_context.API.GetCurrentTheme());
        }

        public List<Result> Query(Query query)
        {
            if (query == null)
            {
                throw new ArgumentNullException(paramName: nameof(query));
            }

            var results = new List<Result>();

            if (query.Search == "ound")
            {
                results.Add(GetResult());
            }

            return results;
        }

        private Result GetResult()
        {
            return new Result
            {
                ContextData = string.Empty,
                Title = string.Format("Test!"),
                IcoPath = _icon_path,
                Score = 300,
                SubTitle = string.Format(Properties.Resources.copy_to_clipboard, "1"),
                Action = c =>
                {
                    var ret = false;
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            Clipboard.SetText("1");
                            ret = true;
                        }
                        catch (ExternalException)
                        {
                            MessageBox.Show(Properties.Resources.copy_failed);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    return ret;
                },
            };
        }

        private ContextMenuResult CreateContextMenuEntry(string result)
        {
            return new ContextMenuResult
            {
                PluginName = Name,
                Title = Properties.Resources.context_menu_copy,
                Glyph = "\xE8C8",
                FontFamily = "Segoe MDL2 Assets",
                AcceleratorKey = Key.Enter,
                Action = _ =>
                {
                    bool ret = false;
                    var thread = new Thread(() =>
                    {
                        try
                        {
                            Clipboard.SetText(result);
                            ret = true;
                        }
                        catch (ExternalException)
                        {
                            MessageBox.Show(Properties.Resources.copy_failed);
                        }
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                    return ret;
                },
            };
        }

        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (!(selectedResult?.ContextData is string))
            {
                return new List<ContextMenuResult>();
            }

            List<ContextMenuResult> contextResults = new List<ContextMenuResult>();
            string result = selectedResult.ContextData as string;
            contextResults.Add(CreateContextMenuEntry(result));

            return contextResults;
        }

        public string GetTranslatedPluginTitle()
        {
            return Properties.Resources.plugin_name;
        }

        public string GetTranslatedPluginDescription()
        {
            return Properties.Resources.plugin_description;
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }

        private static void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                _icon_path = "Images/unitconverter.light.png";
            }
            else
            {
                _icon_path = "Images/unitconverter.dark.png";
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_context != null && _context.API != null)
                    {
                        _context.API.ThemeChanged -= OnThemeChanged;
                    }

                    _disposed = true;
                }
            }
        }
    }
}
