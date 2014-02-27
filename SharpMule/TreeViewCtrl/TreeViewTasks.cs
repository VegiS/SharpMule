using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections; 
using System.Windows.Controls;
using TestManager.Shared;



namespace TestManager.TreeViewCtrl
{

    public class TreeItem<T>
    {
        public TreeItem()
        {
            Children = new List<TreeItem<T>>();
        }

        public void AddChild(T data)
        {
            Children.Add(new TreeItem<T> { Data = data, Parent = this });
            Count++; 
        }


        public List<TreeItem<T>> Children { get; set; }
        public TreeItem<T> Parent { get; set; }
        public T Data { get; set; }
        public int Count { get; set; }
    }

    public class TreeViewTasks
    {
        const bool IS_TREE_EXPENDED = true;
        const bool IS_SUITES_EXPENDED = true; 
        public static string TestPath { get; set; }
        public static string DEFAULTSCRIPT =@"main.tc";
        public static TreeItem<string> treeroot;
        public static Dictionary<string, ArrayList> TestTreeMap = new Dictionary<string, ArrayList>(); 


        public static void GetTestDirectories(System.Windows.Controls.TreeView tv,int index)
        {
            
            DirectoryInfo dirInfo = new DirectoryInfo(SharedTasks.Projects.ProjectList[index].Path);
            if (dirInfo.Exists)
            {
                TreeViewItem root = new TreeViewItem();
                
                root.Header = dirInfo.Name;
                root.Tag = dirInfo.FullName;
                root.IsSelected = true;
                root.IsExpanded = IS_TREE_EXPENDED;

                // New Code
                treeroot = new TreeItem<string> { Data = dirInfo.FullName };
                

                tv.Items.Clear();
                tv.Items.Add(root);


                //LoadUp all Test Projects
                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    // new Code
                    treeroot.AddChild(dir.FullName + "\\");

                    TreeViewItem tvi = new TreeViewItem();
                    
                    tvi.Header = dir.Name;
                    tvi.Tag = dir.FullName + "\\";
                    tvi.IsExpanded = IS_SUITES_EXPENDED;

                    root.Items.Add(tvi);

                    LoadSubDirs(tvi, dir, treeroot);
                }

            }
            else
            {
                MainWindow.MessageBx("Project doesnt seem to exist. Check project path in the config file"); 
            }

        }

        static void LoadSubDirs(TreeViewItem parent, DirectoryInfo parentDir,TreeItem<string> treeparent)
        {
            DirectoryInfo[] directories = parentDir.GetDirectories();

            //Change the color of th test cases. 
            if(directories.Length == 0)
                parent.Foreground = System.Windows.Media.Brushes.Brown; 
            


            foreach (DirectoryInfo dir in directories)
            {
                // new code
                treeparent.AddChild(dir.FullName + "\\");

                TreeViewItem child = new TreeViewItem();
                child.Header = dir.Name;
                child.Tag = dir.FullName+"\\";
                child.IsExpanded = IS_SUITES_EXPENDED; 

                parent.Items.Add(child);
                

                LoadSubDirs(child, dir,treeparent);
                
            }
        }
        public static bool IsScriptSelected()
        {
            
            if (File.Exists(TestPath+DEFAULTSCRIPT))
                return true;

            return false; 
        }



        public static string GetScriptContent()
        {

            if (IsScriptSelected())
            {
                SharedTasks.Editor.Internal.FilePath = TestPath + DEFAULTSCRIPT;
                SharedTasks.Editor.Internal.Text = File.ReadAllText(SharedTasks.Editor.Internal.FilePath);

                return SharedTasks.Editor.Internal.Text;

            }

            return String.Empty;
        }

        public static string GetSpecificContent(string filename)
        {
            if (File.Exists(TestPath + filename))
            {
                SharedTasks.Editor.Internal.FilePath = TestPath + filename;
                SharedTasks.Editor.Internal.Text = File.ReadAllText(TestPath + filename);

                return SharedTasks.Editor.Internal.Text; 
            }

            return String.Empty; 
        }


      

       

    }
}
