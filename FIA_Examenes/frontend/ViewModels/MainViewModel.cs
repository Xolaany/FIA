using CommunityToolkit.Mvvm.ComponentModel;

namespace frontend.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Welcome to Avalonia!";
    [ObservableProperty]
    public partial string GGS {get; set; } = "TEXTO RANDOM";
}
