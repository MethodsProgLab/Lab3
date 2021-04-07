using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MenuManager;

namespace GUI
{
    public partial class MenuForm : Form
    {
        private readonly MenuHandler _menu;
        private bool _visibility = true;
        private byte _levelInvisibility;
        private string _pathToMenu = "";
        private object _dll;

        public MenuForm()
        {
            InitializeComponent();
            _menu = new MenuHandler();

            treeView1.ShowLines = false;
            treeView1.ShowPlusMinus = false;
        }

        private void RenderMenu()
        {
            treeView1.BeginUpdate();
            treeView1.Nodes.Clear();

            foreach (Item item in _menu)
            {
                AddInTree(item.Level, item.Name, item.Status);
            }

            treeView1.EndUpdate();
            treeView1.ExpandAll();
        }

        private void LoadDll(string pathToDll)
        {
            var assembly = Assembly.LoadFrom(pathToDll);
            var type = assembly.GetType("MenuManager.MenuHandler", true, true);
            _dll = Activator.CreateInstance(type!);

            var method = type.GetMethod("Load");
            using var reader = new StreamReader(_pathToMenu);
            method!.Invoke(_dll, new object[] { reader });
        }

        private void AddInTree(byte level, string name, NodeStatus status)
        {
            if (!_visibility)
            {
                if (level > _levelInvisibility)
                {
                    return;
                }

                _visibility = true;
            }

            var node = new TreeNode(name);

            switch (status)
            {
                case NodeStatus.Disabled:
                    node.ForeColor = Color.Gray;
                    break;
                case NodeStatus.Hidden:
                    _visibility = false;
                    _levelInvisibility = level;
                    break;
            }

            if (level == 0)
            {
                treeView1.Nodes.Add(node);
                treeView1.SelectedNode = node;
            }
            else
            {
                var currentRoot = treeView1.SelectedNode;
                try
                {
                    for (var i = 1; i < level; i++)
                    {
                        currentRoot = currentRoot.LastNode;
                    }
                    currentRoot.Nodes.Add(node);
                }
                catch (NullReferenceException)
                {
                    return;
                }
            }

            if (status == NodeStatus.Hidden)
            {
                treeView1.Nodes.Remove(node);
            }
        }

        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var element = _menu
                .Cast<Item>().First(node => node.Name == e.Node.Text);

            if (element.Status != NodeStatus.Normal)
            {
                return;
            }
            
            switch (element.NameMethod)
            {
                case "SetColorWhite":
                    SetColor(Color.White);
                    break;
                case "SetColorRed":
                    SetColor(Color.Red);
                    break;
                case "SetColorYellow":
                    SetColor(Color.Yellow);
                    break;
                case "SetColorBlue":
                    SetColor(Color.Blue);
                    break;
            }
        }

        private void SetColor(Color color)
        {
            treeView1.BackColor = color;
        }

        private void OpenMenuItemClick(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            
            using var stream = openFileDialog.OpenFile();

            try
            {
                _menu.Load(stream);
            }
            catch (ArgumentException)
            {
                MessageBox.Show("Не удалось загрузить дерево. Некорректные входные данные.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            _pathToMenu = openFileDialog.FileName;
            RenderMenu();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var form = new AddForm();

            if (form.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var newItem = new Item
            {
                Name = form.NameElement,
                Level = form.Level,
                NameMethod = form.MethodName,
                Status = NodeStatus.Normal
            };

            if (_menu.ListItem.Count == 0)
            {
                _menu.ListItem.Add(newItem);
            } 
            else
            {
                var item = _menu.Cast<Item>().First(pred => pred.Name == treeView1.SelectedNode.Text);
                var index = _menu.ListItem.IndexOf(item);

                _menu.Add(index + 1, newItem);

            }

            RenderMenu();
        }

        private void changeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = _menu.Cast<Item>().First(pred => pred.Name == treeView1.SelectedNode.Text);
            var index = _menu.ListItem.IndexOf(item);

            item.Status = item.Status == NodeStatus.Normal ? NodeStatus.Disabled : NodeStatus.Normal;

            _menu[index] = item;
            RenderMenu();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = _menu.Cast<Item>().First(pred => pred.Name == treeView1.SelectedNode.Text);
            var index = _menu.ListItem.IndexOf(item);

            _menu.Delete(index);
            RenderMenu();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var stream = saveFileDialog.OpenFile();
            _menu.Save(stream);
        }

        private void dLLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openDllDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (_pathToMenu.Length == 0)
            {
                MessageBox.Show("Сначала нужно открыть файл!");
                return;
            }

            try
            {
                LoadDll(openDllDialog.FileName);
            }
            catch (Exception)
            {
                MessageBox.Show("Не удалось загрузить библиотеку!", "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
