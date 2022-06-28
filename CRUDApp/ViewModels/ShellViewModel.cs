using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CRUDApp.Contracts.Services;
using CRUDApp.Properties;

using MahApps.Metro.Controls;
using MahApps.Metro.IconPacks;

using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace CRUDApp.ViewModels
{
    public class ShellViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private HamburgerMenuItem _selectedMenuItem;
        private HamburgerMenuItem _selectedOptionsMenuItem;
        private RelayCommand _goBackCommand;
        private ICommand _menuItemInvokedCommand;
        private ICommand _optionsMenuItemInvokedCommand;
        private ICommand _loadedCommand;
        private ICommand _unloadedCommand;

        public HamburgerMenuItem SelectedMenuItem
        {
            get { return _selectedMenuItem; }
            set { SetProperty(ref _selectedMenuItem, value); }
        }

        public HamburgerMenuItem SelectedOptionsMenuItem
        {
            get { return _selectedOptionsMenuItem; }
            set { SetProperty(ref _selectedOptionsMenuItem, value); }
        }

        // TODO: Change the icons and titles for all HamburgerMenuItems here.
        // Changed to Icon.

        public ObservableCollection<HamburgerMenuItem> MenuItemsAdmin { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            //var packIconMaterial = new PackIconEntypo()
            //{
            //    Kind = PackIconMaterialKind.Cookie,
            //    Margin = new Thickness(4, 4, 2, 4),
            //    Width = 24,
            //    Height = 24,
            //    VerticalAlignment = VerticalAlignment.Center
            //};
            new HamburgerMenuGlyphItem() { Label = "DB Creation", Glyph = "\uF156", TargetPageType = typeof(MainViewModel) },
            //new HamburgerMenuGlyphItem() { Label = Resources.ShellListDetailsPage, Glyph = "\uE8A5", TargetPageType = typeof(ListDetailsViewModel) },
            //new HamburgerMenuGlyphItem() { Label = Resources.ShellContentGridPage, Glyph = "\uE8A5", TargetPageType = typeof(ContentGridViewModel) },
            new HamburgerMenuGlyphItem() { Label = "CRUD", Glyph = "\uF103", TargetPageType = typeof(DataGridViewModel) },
            new HamburgerMenuGlyphItem() { Label = "Users", Glyph = "\uE716", TargetPageType = typeof(BlankViewModel) },
        };

        public ObservableCollection<HamburgerMenuItem> MenuItemsUser { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
           new HamburgerMenuGlyphItem() { Label = "DB Creation", Glyph = "\uF156", TargetPageType = typeof(MainViewModel) },
            //new HamburgerMenuGlyphItem() { Label = Resources.ShellListDetailsPage, Glyph = "\uE8A5", TargetPageType = typeof(ListDetailsViewModel) },
            //new HamburgerMenuGlyphItem() { Label = Resources.ShellContentGridPage, Glyph = "\uE8A5", TargetPageType = typeof(ContentGridViewModel) },
            new HamburgerMenuGlyphItem() { Label = "CRUD", Glyph = "\uF103", TargetPageType = typeof(DataGridViewModel) },
            //new HamburgerMenuGlyphItem() { Label = "Users", Glyph = "\uE8A5", TargetPageType = typeof(BlankViewModel) },
        };

        public ObservableCollection<HamburgerMenuItem> _MenuItems;
        public ObservableCollection<HamburgerMenuItem> MenuItems { get { if (CRUDApp.App.role == "admin") { return MenuItemsAdmin; } else {return MenuItemsUser; } } set { if (CRUDApp.App.role == "admin") { _MenuItems = value; } else { _MenuItems = value; } } }

        public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } = new ObservableCollection<HamburgerMenuItem>()
        {
            new HamburgerMenuGlyphItem() { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
        };

        public RelayCommand GoBackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(OnGoBack, CanGoBack));

        public ICommand MenuItemInvokedCommand => _menuItemInvokedCommand ?? (_menuItemInvokedCommand = new RelayCommand(OnMenuItemInvoked));

        public ICommand OptionsMenuItemInvokedCommand => _optionsMenuItemInvokedCommand ?? (_optionsMenuItemInvokedCommand = new RelayCommand(OnOptionsMenuItemInvoked));

        public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        public ICommand UnloadedCommand => _unloadedCommand ?? (_unloadedCommand = new RelayCommand(OnUnloaded));

        public ShellViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        private void OnLoaded()
        {
            _navigationService.Navigated += OnNavigated;
        }

        private void OnUnloaded()
        {
            _navigationService.Navigated -= OnNavigated;
        }

        private bool CanGoBack()
            => _navigationService.CanGoBack;

        private void OnGoBack()
            => _navigationService.GoBack();

        private void OnMenuItemInvoked()
            => NavigateTo(SelectedMenuItem.TargetPageType);

        private void OnOptionsMenuItemInvoked()
            => NavigateTo(SelectedOptionsMenuItem.TargetPageType);

        private void NavigateTo(Type targetViewModel)
        {
            if (targetViewModel != null)
            {
                _navigationService.NavigateTo(targetViewModel.FullName);
                if (CRUDApp.App.role == "admin")
                {
                    this.MenuItems = this.MenuItemsAdmin;
                }
                else
                {
                    this.MenuItems = this.MenuItemsUser;
                }
            }
        }

        private void OnNavigated(object sender, string viewModelName)
        {
            var item = new HamburgerMenuItem();
            if (CRUDApp.App.role == "admin")
            {
                item = MenuItemsAdmin
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            } else
            {
                item = MenuItemsUser
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            }
            
            if (item != null)
            {
                SelectedMenuItem = item;
            }
            else
            {
                SelectedOptionsMenuItem = OptionMenuItems
                        .OfType<HamburgerMenuItem>()
                        .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            }

            GoBackCommand.NotifyCanExecuteChanged();
        }
    }
}
