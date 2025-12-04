// GUI/ViewModels/SelectableItem.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace GUI.ViewModels
{
    public partial class SelectableItem : ObservableObject
    {
        public string Name { get; set; } // Tên môn học

        [ObservableProperty]
        private bool _isSelected; // Trạng thái được chọn hay chưa

        public SelectableItem(string name, bool isSelected = false)
        {
            Name = name;
            IsSelected = isSelected;
        }
    }
}