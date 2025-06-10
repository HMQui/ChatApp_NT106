using ChatApp.Client.Models;
using ChatApp.Client.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ChatApp.Client.Views
{
    public partial class GroupInfoWindow : Window
    {
        private readonly int _groupId;
        private readonly GroupService _groupService;
        private readonly UserService _userService;
        private List<GroupMemberDTO> _members;
        private string _originalGroupName;

        public GroupInfoWindow(int groupId)
        {
            InitializeComponent();
            _groupId = groupId;
            _groupService = new GroupService();
            _userService = new UserService();
            LoadGroupInfo();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void LoadGroupInfo()
        {
            try
            {
                var group = _groupService.GetGroupById(_groupId);
                if (group != null)
                {
                    _originalGroupName = group.GroupName;
                    GroupNameTextBox.Text = group.GroupName;
                    await LoadMembers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading group info: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadMembers()
        {
            try
            {
                _members = _groupService.GetGroupMembers(_groupId);
                await Dispatcher.InvokeAsync(() =>
                {
                    MembersList.Children.Clear();

                    foreach (var member in _members)
                    {
                        var user = _userService.GetUserById(member.UserId);
                        if (user != null)
                        {
                            var memberPanel = new Border
                            {
                                Background = Brushes.White,
                                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                                BorderThickness = new Thickness(1),
                                CornerRadius = new CornerRadius(8),
                                Padding = new Thickness(10),
                                Margin = new Thickness(0, 5, 0, 5)
                            };

                            var grid = new Grid();
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
                            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            // Avatar
                            var avatar = new Image
                            {
                                Source = new BitmapImage(new Uri(user.AvatarUrl ?? "pack://application:,,,/Resources/default_avatar.png")),
                                Width = 40,
                                Height = 40,
                                Stretch = Stretch.UniformToFill
                            };
                            Grid.SetColumn(avatar, 0);

                            // Name and Email
                            var nameStack = new StackPanel
                            {
                                VerticalAlignment = VerticalAlignment.Center,
                                Margin = new Thickness(10, 0, 0, 0)
                            };
                            Grid.SetColumn(nameStack, 1);

                            var nameText = new TextBlock
                            {
                                Text = user.FullName,
                                FontWeight = FontWeights.SemiBold,
                                FontSize = 14,
                                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
                            };

                            var emailText = new TextBlock
                            {
                                Text = user.Email,
                                FontSize = 12,
                                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102))
                            };

                            nameStack.Children.Add(nameText);
                            nameStack.Children.Add(emailText);

                            grid.Children.Add(avatar);
                            grid.Children.Add(nameStack);

                            memberPanel.Child = grid;
                            MembersList.Children.Add(memberPanel);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
                {
                    MessageBox.Show("Group name cannot be empty");
                    return;
                }

                if (GroupNameTextBox.Text != _originalGroupName)
                {
                    var success = await _groupService.UpdateGroupName(_groupId, GroupNameTextBox.Text);
                    if (success)
                    {
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update group name");
                    }
                }
                else
                {
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving group info: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 