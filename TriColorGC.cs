using System;
using System.Collections.Generic;
using System.Linq;

class TriColorGC
{
    /*
     root --> 1 --> 2 --> 3
          --> 4
          --> 5 --> 6
         
     root --> 1 --> 2 -|-> 3
          --> 4
          -|-> 5 --> 6
     */
    static void Main(string[] args)
    {
        var global = new MyGlobal();

        var obj1 = global.Spawn("obj1");
        var obj2 = obj1.Spawn("obj2");
        var obj3 = obj2.Spawn("obj3");
        var obj4 = global.Spawn("obj4");
        var obj5 = global.Spawn("obj5");
        var obj6 = obj5.Spawn("obj6");

        obj3.Suicide();
        obj5.Suicide();

        global.GC();
    }

    class MyGlobal : IParentable
    {
        private List<MyObject> allObjs = new List<MyObject>();
        private List<MyObject> root = new List<MyObject>();

        public MyObject Spawn(string name)
        {
            var baby = new MyObject(name, this);
            allObjs.Add(baby);
            root.Add(baby);
            return baby;
        }

        public void Kill(MyObject child)
        {
            root.Remove(child);
        }

        public void GC()
        {
            Mark();
            Sweep();
        }

        public void AddToAll(MyObject obj)
        {
            allObjs.Add(obj);
        }

        private void Sweep()
        {
            foreach (var item in allObjs)
            {
                if (item.Color == Color.White)
                {
                    Console.WriteLine("Sweep " + item.Name);
                }
            }
        }

        private void Mark()
        {
            foreach (var gray in root)
            {
                gray.Color = Color.Gray;
            }

            foreach (var obj in allObjs)
            {
                if (obj.Color != Color.Gray)
                {
                    obj.Color = Color.White;
                }
            }

            PrintAllObjs("phrase 1");

            var graySet = allObjs.Where(obj => obj.Color == Color.Gray);
            while (graySet.Count() > 0)
            {
                foreach (var gray in graySet)
                {
                    gray.Color = Color.Black;

                    foreach (var child in gray.Children)
                    {
                        if (child.Color == Color.White)
                        {
                            child.Color = Color.Gray;
                        }
                    }
                }

                graySet = allObjs.Where(obj => obj.Color == Color.Gray);
            }

            PrintAllObjs("phrase 2");
        }

        private void PrintAllObjs(string phrase)
        {
            Console.WriteLine("After " + phrase + " done:");

            foreach (var obj in allObjs)
            {
                Console.WriteLine(string.Format("{0}'s color: {1}", obj.Name, obj.Color));
            }

            Console.WriteLine();
        }
    }

    class MyObject : IParentable
    {
        public Color Color { get; set; }
        public string Name { get; private set; }
        public IParentable Parent { get; private set; }
        public List<MyObject> Children { get; private set; }

        public MyObject Spawn(string name)
        {
            var baby = new MyObject(name, this);
            Children.Add(baby);

            IParentable root = Parent;
            while (!(root is MyGlobal))
            {
                root = ((MyObject)root).Parent;
            }
            ((MyGlobal)root).AddToAll(baby);

            return baby;
        }

        public void Suicide()
        {
            Parent.Kill(this);
        }

        public void Kill(MyObject child)
        {
            Children.Remove(child);
        }

        public MyObject(string name, IParentable parent)
        {
            this.Name = name;
            this.Parent = parent;
            Children = new List<MyObject>();
        }
    }

    interface IParentable
    {
        void Kill(MyObject child);
        MyObject Spawn(string name);
    }

    enum Color
    {
        Black,
        White,
        Gray
    }
}
