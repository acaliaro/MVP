﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MVP.Models;
using MVP.Pages;
using MVP.Services.Interfaces;
using MVP.ViewModels.Data;
using MvvmHelpers;
using TinyMvvm;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MVP.ViewModels
{
    public class ContributionTypePickerViewModel : BaseViewModel
    {
        ContributionViewModel contribution;

        public ICommand SelectContributionTypeCommand { get; set; }

        public List<ContributionTypeViewModel> ContributionTypes { get; set; } = new List<ContributionTypeViewModel>();

        public ContributionTypePickerViewModel(IAnalyticsService analyticsService, IAuthService authService,
            IDialogService dialogService, INavigationHelper navigationHelper)
            : base(analyticsService, authService, dialogService, navigationHelper)
        {
            SelectContributionTypeCommand = new Command<ContributionTypeViewModel>((x) => SelectContributionType(x));
        }

        public async override Task Initialize()
        {
            await base.Initialize();

            // If a new contribution is coming in, the user created one from the URL
            // they had on the clipboard.
            if (NavigationParameter is ContributionViewModel contribution)
            {
                this.contribution = contribution;
            }

            LoadContributionTypes().SafeFireAndForget();
        }

        public async override Task Back()
            => await NavigationHelper.BackAsync(ContributionTypes.FirstOrDefault(x => x.IsSelected)?.ContributionType);

        async Task LoadContributionTypes()
        {
            if (Connectivity.NetworkAccess == NetworkAccess.Internet)
            {
                var types = await MvpApiService.GetContributionTypesAsync().ConfigureAwait(false);

                if (types != null)
                {
                    ContributionTypes = types
                        .OrderBy(x => x.Name)
                        .Select(x => new ContributionTypeViewModel
                        {
                            ContributionType = x
                        }).ToList();

                    // Editing mode
                    if (contribution.ContributionType.Value != null)
                    {
                        var selected = ContributionTypes.FirstOrDefault(x => x.ContributionType.Id == contribution.ContributionType.Value.Id);
                        selected.IsSelected = true;
                    }
                }
            }
        }

        void SelectContributionType(ContributionTypeViewModel vm)
        {
            if (vm == null)
                return;

            foreach (var item in ContributionTypes)
                item.IsSelected = false;

            vm.IsSelected = true;
        }
    }
}