using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BigTed;
using Foundation;
using MonoTouch.Dialog;
using UIKit;

namespace BTProgressHUDDemo
{
    public class MainViewController : UITableViewController, IUITableViewDelegate, IUITableViewDataSource
    {
        float _progress = -1;
        NSTimer _timer;

        List<string> _listViewItems = new List<string>();
        Dictionary<string, Action> _actionItems = new Dictionary<string, Action>();
        Dictionary<string, Func<Task>> _taskItems = new Dictionary<string, Func<Task>>();

        public const string CellIdentifier = "BTProgessHUDCell";

        public MainViewController() : base(UITableViewStyle.Plain)
        {
            Title = "BTProgessHUD";

        }

        void AddRow(string name, Action action)
        {
            _listViewItems.Add(name);
            _actionItems.Add(name, action);
        }

        void AddRow(string name, Func<Task> task)
        {
            _listViewItems.Add(name);
            _taskItems.Add(name, task);
        }

        void InvalidateTimer()
        {
            if (_timer != null)
            {
                _timer.Invalidate();
                _timer = null;
            }
        }

        public override void LoadView()
        {
            base.LoadView();


            AddRow("Masks", () =>
            {
                ProgressHUD.Shared.Show("Loading...", -1, ProgressHUD.MaskType.Black, 0.75);
                KillAfter(5);
            });

            AddRow("Async", async () =>
            {
                ProgressHUD.Shared.Show("Logging in.");
                var res = await BackgroundSleepOperation();
                ProgressHUD.Shared.Dismiss();
            });

            AddRow("Show Continuous Progress", () =>
            {
                ProgressHUD.Shared.Ring.Color = UIColor.Green;
                ProgressHUD.Shared.ShowContinuousProgress("Continuous progress...", ProgressHUD.MaskType.None, 1000);
                KillAfter(3);
            });
            
            AddRow("Show Continuous Progress with Image", () =>
            {
                ProgressHUD.Shared.Ring.Color = UIColor.Green;
                ProgressHUD.Shared.ShowContinuousProgress("Continuous progress...", ProgressHUD.MaskType.None, 1000, UIImage.FromBundle("xamarin_icon.png"));
                KillAfter(3);
            });

            AddRow("Show", () => {
                ProgressHUD.Shared.Show();
                KillAfter();
            });

            AddRow("Show BT", () => {
                BTProgressHUD.Show();
                InvalidateTimer();
                 _timer = NSTimer.CreateRepeatingTimer(2f, delegate
                {
                    BTProgressHUD.Dismiss();
                    InvalidateTimer();
                });
                NSRunLoop.Current.AddTimer(_timer, NSRunLoopMode.Common);
            });

            AddRow("Show with Cancel", () => {
                ProgressHUD.Shared.Show("Cancel Me", () => {
                    ProgressHUD.Shared.ShowErrorWithStatus("Operation Cancelled!");
                }, "Please Wait");
                //KillAfter ();
            });

            AddRow("Show inside Alert", () => {

                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    var alert = UIAlertController.Create("Oh, Hai2", "Press the button to show it.", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                    alert.AddAction(UIAlertAction.Create("Show the HUD", UIAlertActionStyle.Default, (alertAction) =>
                    {
                        ProgressHUD.Shared.Show("this should never go away?");
                        KillAfter();
                    }));

                    PresentViewController(alert, true, null);
                }
                else
                {
#pragma warning disable 0618 // Disable obsolete warning.
                    var alert = new UIAlertView("Oh, Hai", "Press the button to show it.", null, "Cancel", "Show the HUD");
#pragma warning restore 0618

                    alert.Clicked += (object sender, UIButtonEventArgs e) => {
                        if (e.ButtonIndex == 0)
                            return;
                        ProgressHUD.Shared.Show("this should never go away?");
                        KillAfter();
                    };
                    alert.Show();
                }
               
            });

            AddRow("Show Message", () => {
                ProgressHUD.Shared.Show(status: "Processing your image");
                KillAfter();
            });

            AddRow("Show Success", () => {
                ProgressHUD.Shared.ShowSuccessWithStatus("Great success!");
            });

            AddRow("Show Fail", () => {
                ProgressHUD.Shared.ShowErrorWithStatus("Oh, thats bad");
            });

            AddRow("Show Fail 5 seconds", () => {
                ProgressHUD.Shared.ShowErrorWithStatus("Oh, thats bad", timeoutMs: 5000);
            });

            AddRow("Toast Top", () => {
                //ProgressHUD.Shared.HudForegroundColor = UIColor.White;
                //ProgressHUD.Shared.HudToastBackgroundColor = UIColor.DarkGray;
                ProgressHUD.Shared.ShowToast("Hello from the toast\nLine 2", toastPosition: ProgressHUD.ToastPosition.Top, timeoutMs: 3000);
            });

            AddRow("Toast Center", () => {
                //ProgressHUD.Shared.HudForegroundColor = UIColor.White;
                //ProgressHUD.Shared.HudToastBackgroundColor = UIColor.DarkGray;
                ProgressHUD.Shared.ShowToast("Hello from the toast\r\nLine 2", toastPosition: ProgressHUD.ToastPosition.Center, timeoutMs: 3000);
            });

            AddRow("Toast Bottom", () => {
                //ProgressHUD.Shared.HudForegroundColor = UIColor.White;
                //ProgressHUD.Shared.HudToastBackgroundColor = UIColor.DarkGray;
                ProgressHUD.Shared.ShowToast("Hello from the toast\r\nLine 2", toastPosition: ProgressHUD.ToastPosition.Bottom, timeoutMs: 3000);
            });

            AddRow("Progress", () => {
                _progress = 0;
                ProgressHUD.Shared.Show("Hello!", _progress);
                InvalidateTimer();
                _timer = NSTimer.CreateRepeatingTimer(0.5f, delegate
                {
                    _progress += 0.1f;
                    if (_progress > 1)
                    {
                        InvalidateTimer();
                        ProgressHUD.Shared.Dismiss();
                    }
                    else
                    {
                        ProgressHUD.Shared.Show("Hello!", _progress);
                    }


                });
                NSRunLoop.Current.AddTimer(_timer, NSRunLoopMode.Common);
            });

            AddRow("Dismiss", () => {
                ProgressHUD.Shared.Dismiss();
                InvalidateTimer();
            });

            //From a bug report from Jose
            AddRow("Show, Dismiss, remove cancel", async () => {
                ProgressHUD.Shared.Show("Cancel", () => {
                    Console.WriteLine("Canceled.");
                }, "Please wait", -1, ProgressHUD.MaskType.Black);

                var result = await BackgroundSleepOperation();

                ProgressHUD.Shared.Dismiss();

                ProgressHUD.Shared.ShowSuccessWithStatus("Done", 2000);
            });
            
            TableView.Delegate = this;
            TableView.DataSource = this;
        }


        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return _listViewItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(CellIdentifier);

            // if there are no cells to reuse, create a new one
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellIdentifier);
            }

            cell.TextLabel.Text = _listViewItems[indexPath.Row];

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, true);

            var name = _listViewItems[indexPath.Row];
            if (_actionItems.ContainsKey(name))
            {
                _actionItems[name].Invoke();
            }
            else if (_taskItems.ContainsKey(name))
            {
                _taskItems[name].Invoke();
            }
        }


        async Task<bool> BackgroundSleepOperation()
        {
            return await Task.Run(async () => {
                await Task.Delay(1000);
                return true;
            });
        }





        void KillAfter(float timeout = 2)
        {
            InvalidateTimer();
            _timer = NSTimer.CreateRepeatingTimer(timeout, delegate
            {
                ProgressHUD.Shared.Dismiss();
                InvalidateTimer();
            });
            NSRunLoop.Current.AddTimer(_timer, NSRunLoopMode.Common);
        }


    }
}
