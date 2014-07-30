﻿using System;
using System.Windows.Forms;
using PPExtraEventHelper;

namespace PowerPointLabs.Views
{
    static class FolderDialogLauncher
    {
        /// <summary>
        /// Using title text to look for the top level dialog window is fragile.
        /// In particular, this will fail in non-English applications.
        /// </summary>
        private const string TopLevelSearchString = "Browse For Folder";

        /// <summary>
        /// These should be more robust.  We find the correct child controls in the dialog
        /// by using the GetDlgItem method, rather than the FindWindow(Ex) method,
        /// because the dialog item IDs should be constant.
        /// </summary>
        private const int DlgItemBrowseControl = 0;
        private const int DlgItemTreeView = 100;

        private static int _retries = 10;

        /// <summary>
        /// Calling this method is identical to calling the ShowDialog method of the provided
        /// FolderBrowserDialog, except that an attempt will be made to scroll the Tree View
        /// to make the currently selected folder visible in the dialog window.
        /// </summary>
        /// <param name="dlg"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static DialogResult ShowFolderBrowser(FolderBrowserDialog dlg, IWin32Window parent = null)
        {
            DialogResult result;

            using (var timer = new Timer())
            {
                timer.Tick += TimerTickHandler;
                timer.Interval = 10;
                timer.Start();

                result = dlg.ShowDialog(parent);
            }

            _retries = 10;
            return result;
        }

        private static void TimerTickHandler(object sender, EventArgs args)
        {
            var timer = sender as Timer;

            if (_retries > 0)
            {
                --_retries;
                var hwndDlg = Native.FindWindow(null, TopLevelSearchString);
                if (hwndDlg != IntPtr.Zero)
                {
                    var hwndFolderCtrl = Native.GetDlgItem(hwndDlg, DlgItemBrowseControl);
                    if (hwndFolderCtrl != IntPtr.Zero)
                    {
                        var hwndTreeView = Native.GetDlgItem(hwndFolderCtrl, DlgItemTreeView);

                        if (hwndTreeView != IntPtr.Zero)
                        {
                            var item = Native.SendMessage(hwndTreeView, (uint)Native.Message.TVM_GETNEXTITEM,
                                                          new IntPtr((uint)Native.Message.TVGN_CARET),
                                                          IntPtr.Zero);
                            if (item != IntPtr.Zero)
                            {
                                Native.SendMessage(hwndTreeView, (uint)Native.Message.TVM_ENSUREVISIBLE, IntPtr.Zero, item);
                                _retries = 0;
                                timer.Stop();
                            }
                        }
                    }
                }
            }
            else
            {
                //  We failed to find the Tree View control.
                //
                //  As a fall back (and this is an UberUgly hack), we will send
                //  some fake keystrokes to the application in an attempt to force
                //  the Tree View to scroll to the selected item.
                timer.Stop();
                SendKeys.Send("{TAB}{TAB}{DOWN}{UP}");
            }
        }
    }
}
