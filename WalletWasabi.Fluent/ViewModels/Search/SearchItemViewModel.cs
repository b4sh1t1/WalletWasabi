using System;
using ReactiveUI;
using WalletWasabi.Fluent.ViewModels.NavBar;
using WalletWasabi.Fluent.ViewModels.Navigation;

namespace WalletWasabi.Fluent.ViewModels.Search
{
	public class SearchItemViewModel : NavBarItemViewModel
	{
		public SearchItemViewModel(
			string title,
			string caption,
			int order,
			SearchCategory category,
			string keywords,
			string iconName,
			NavigationStateViewModel navigationState,
			NavigationTarget navigationTarget,
			Func<RoutableViewModel> createTargetView) : base(navigationState, navigationTarget)
		{
			Title = title;
			Caption = caption;
			Order = order;
			Category = category;
			Keywords = keywords;
			IconName = iconName;
			OpenCommand = ReactiveCommand.Create(() => NavigateTo(createTargetView(), navigationTarget));
		}

		public override string IconName { get; }

		public string Caption { get; }

		public int Order { get; }

		public SearchCategory Category { get; }

		public string Keywords { get; }
	}
}