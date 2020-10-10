using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gaze
{
    public class TreeState
    {
        // Native functions to store the scroll position
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetScrollPos(int hWnd, int nBar);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        private const int SB_HORZ = 0x0;
        private const int SB_VERT = 0x1;

        private int m_previousScrollPositionX = 0;
        private int m_previousScrollPositionY = 0;

        private List<string> m_expandedNodes = new List<string>();
        private string m_selectedNode;

        private bool m_isRestoring = false;

        public TreeState()
        {
        }

        public void SaveState(TreeView tree, string filename)
        {
            Store(tree);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Selected:" + m_selectedNode);

            foreach (string nodePath in m_expandedNodes)
            {
                sb.AppendLine(nodePath);
            }

            File.WriteAllText(filename, sb.ToString());
        }

        public void LoadState(TreeView tree, string filename)
        {
            if (File.Exists(filename))
            {
                m_expandedNodes.Clear();

                var lines = File.ReadAllLines(filename).ToList();

                m_selectedNode = lines[0].Substring("Selected:".Length);
                lines.RemoveAt(0);

                m_expandedNodes.AddRange(lines);

                Restore(tree);
            }
        }

        public void Store(TreeView tree)
        {
            if (!m_isRestoring)
            {
                m_expandedNodes.Clear();
                m_selectedNode = tree.SelectedNode != null ? GetPath(tree.SelectedNode) : "None";

                // Run down all of the nodes in the tree seeing which ones are expanded
                foreach (TreeNode node in tree.Nodes)
                {
                    FindExpandedNodes(node);
                }

                m_previousScrollPositionX = GetScrollPos((int)tree.Handle, SB_HORZ);
                m_previousScrollPositionY = GetScrollPos((int)tree.Handle, SB_VERT);
            }
        }

        private void FindExpandedNodes(TreeNode root)
        {
            if (root.IsExpanded)
            {
                m_expandedNodes.Add(GetPath(root));
            }

            foreach (TreeNode child in root.Nodes)
            {
                FindExpandedNodes(child);
            }
        }

        private string GetPath(TreeNode node)
        {
            string path = string.Empty;
            while (node != null)
            {
                path = node.Text + "." + path;
                node = node.Parent;
            }

            return path;
        }

        public void Restore(TreeView tree)
        {
            m_isRestoring = true;

            tree.SuspendLayout();

            foreach (TreeNode node in tree.Nodes)
            {
                ExpandNodes(node);
            }

            if (m_selectedNode != "None")
            {
                tree.SelectedNode = FindNode(tree, m_selectedNode);
            }

            SetScrollPos((IntPtr)tree.Handle, SB_HORZ, m_previousScrollPositionX, true);
            SetScrollPos((IntPtr)tree.Handle, SB_VERT, m_previousScrollPositionY, true);

            tree.ResumeLayout();
            m_isRestoring = false;
        }

        private void ExpandNodes(TreeNode root)
        {
            if (m_expandedNodes.Contains(GetPath(root)))
            {
                root.Expand();
            }

            foreach (TreeNode child in root.Nodes)
            {
                ExpandNodes(child);
            }
        }

        private TreeNode FindNode(TreeView tree, string path)
        {
            foreach (TreeNode node in tree.Nodes)
            {
                TreeNode foundNode = FindNodeRecurse(path, node);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }

        private TreeNode FindNodeRecurse(string path, TreeNode root)
        {
            if (GetPath(root) == path)
            {
                return root;
            }

            foreach (TreeNode child in root.Nodes)
            {
                TreeNode foundNode = FindNodeRecurse(path, child);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null;
        }
    }
}
