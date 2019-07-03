using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void Union()
        {
            var r = new[]
                {
                    new Foo()
                }
                .Union(
                    new[]
                    {
                        new Foo()
                    });
        }

        public void UnionWithEqualityComparer()
        {
            var r = new[]
                {
                    new Foo()
                }
                .Union(
                    new[]
                    {
                        new Foo()
                    },
                    new FakeEqualityComparer<Foo>());
        }

        public void EqualsWithoutEquatable()
        {
            var a = new Foo();
            var b = new Foo();

            var r = a.Equals(b);
        }

        public void EqualsWithEquatable()
        {
            var a = new Equatable();
            var b = new Equatable();

            var r = a.Equals(b);
        }
    }

    public class Sample<T>
    {
        public void DoFail(T t)
        {
            var r = t.Equals(null);
        }

        public void DoSucceed(T t)
        {
            var r = t.Equals(default(T));
        }
    }

    public class Foo
    {
    }

    public class Equatable : IEquatable<Equatable>
    {
        public bool Equals(Equatable other)
        {
            return true;
        }
    }

    public class FakeEqualityComparer<T> : IEqualityComparer<T>
    {
        public bool Equals(T x, T y)
        {
            return true;
        }

        public int GetHashCode(T obj)
        {
            return 0;
        }
    }
}
