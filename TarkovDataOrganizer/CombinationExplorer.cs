using System.Collections;

namespace TarkovDataOrganizer;

public class CombinationExplorer
{

    public class Node : IEnumerator<List<string>>
    {
        public const int EmptyIndex = -1;
        public const string EmptyString = "Empty";
        public Node(List<string> _allowableIDs, bool _emptyAllowed, List<Node> _children)
        {
            AllowedIds = _allowableIDs;
            EmptyAllowed = _emptyAllowed;
            Children = _children;
            if (EmptyAllowed)
                CurrentIndex = EmptyIndex;
            else
                CurrentIndex = 0;
        }
        
        public List<string> AllowedIds;
        public int CurrentIndex;
        
        public List<Node> Children;
        public int CurrentChildIndex;

        public bool EmptyAllowed;


        public bool MoveNext()
        {
            if (CurrentIndex == EmptyIndex)
            {
                CurrentIndex = 0;
                return AllowedIds.Count != 0;
            }

            if (Children[CurrentChildIndex].MoveNext()) 
                return true;
            
            CurrentChildIndex++;
            if (CurrentChildIndex >= Children.Count)
            {
                CurrentIndex++;
                if (CurrentIndex >= AllowedIds.Count)
                    return false;
                CurrentChildIndex = 0;
                Children[CurrentChildIndex].Reset();
                return true;
            }
            Children[CurrentChildIndex].Reset();
            return true;
        }

        public void Reset()
        {
            if (EmptyAllowed)
                CurrentIndex = EmptyIndex;
            else
                CurrentIndex = 0;
        }

        public List<string> Current
        {
            get
            {
                if (CurrentIndex == EmptyIndex)
                    return new List<string>();
                else
                {
                    var current = new List<string>() { AllowedIds[CurrentIndex] };
                    foreach (var child in Children)
                        current.AddRange(child.Current);
                    return current;
                }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}