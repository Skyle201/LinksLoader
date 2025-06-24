using System.ComponentModel;
using System.Runtime.CompilerServices;
using LinksLoader.ViewModels;

namespace LinksLoader.ViewModels
{
    public class WindowViewModel : ViewModelBase
    {
        public TreeViewModel treeViewModel { get; set; } = new TreeViewModel();

        private bool _disableWorksets;
        public bool DisableWorksets
        {
            get => _disableWorksets;
            set { _disableWorksets = value; OnPropertyChanged(); }
        }

        private bool _moveLinksToWorksets;
        public bool MoveLinksToWorksets
        {
            get => _moveLinksToWorksets;
            set { _moveLinksToWorksets = value; OnPropertyChanged(); }
        }
    }
}
