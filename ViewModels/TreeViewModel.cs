using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LinksLoader.ViewModels
{
    public class TreeViewModel : ViewModelBase
    {
        private const string FilePath = @"P:\\!BIM\\04_Настройки, конфигурации\\10_Revit\\Список моделей\\Paths7021.txt";

        public ObservableCollection<TreeNode> RootNodes { get; set; } = new ObservableCollection<TreeNode>();

        private List<string> _originalPaths = new List<string>();

        public TreeViewModel()
        {
            if (File.Exists(FilePath))
            {
                _originalPaths = File.ReadAllLines(FilePath)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .ToList();

                BuildTree(_originalPaths);
            }
        }

        private TreeNode _selectedNode;
        public TreeNode SelectedNode
        {
            get => _selectedNode;
            set
            {
                _selectedNode = value;
                OnPropertyChanged();
            }
        }

        public List<string> GetCheckedPaths()
        {
            var checkedLeafNames = new List<string>();
            CollectCheckedLeafNodes(RootNodes, checkedLeafNames);

            var selectedPaths = _originalPaths
                .Where(p => checkedLeafNames.Any(name => p.EndsWith(name, StringComparison.OrdinalIgnoreCase))).ToList();
            return selectedPaths;
        }


        private void CollectCheckedLeafNodes(IEnumerable<TreeNode> nodes, List<string> collected)
        {
            foreach (var node in nodes)
            {
                if (node.IsChecked && node.Children.Count == 0 && !string.IsNullOrEmpty(node.Name))
                {
                    collected.Add(node.Name);
                }
                CollectCheckedLeafNodes(node.Children, collected);
            }
        }

        private void BuildTree(IEnumerable<string> paths)
        {
            foreach (var fullPath in paths)
            {
                var visualPath = fullPath
                    .Replace(@"RSN:\\", "")
                    .Replace(@"RSN://", "")
                    .Replace("\\", "/");

                var parts = visualPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                ObservableCollection<TreeNode> currentLevel = RootNodes;
                TreeNode currentNode = null;

                foreach (var part in parts)
                {
                    var existing = currentLevel.FirstOrDefault(n => n.Name.Equals(part, StringComparison.OrdinalIgnoreCase));
                    if (existing == null)
                    {
                        var newNode = new TreeNode { Name = part };
                        currentLevel.Add(newNode);
                        currentNode = newNode;
                        currentLevel = newNode.Children;
                    }
                    else
                    {
                        currentNode = existing;
                        currentLevel = existing.Children;
                    }
                }

                if (currentNode != null && currentNode.Children.Count == 0)
                {
                    currentNode.FullPath = fullPath;
                }
            }
        }
    }

    public class TreeNode : ViewModelBase
    {
        private bool _isChecked;

        public string Name { get; set; }
        public string FullPath { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; } = new ObservableCollection<TreeNode>();

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged();
                    foreach (var child in Children)
                        child.IsChecked = value;
                }
            }
        }
    }
}
