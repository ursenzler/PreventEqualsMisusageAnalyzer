using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using PreventEqualsMisusage;

namespace PreventEqualsMisusage.Test
{
    [TestClass]
    public class UnitTest : DiagnosticVerifier
    {
        [TestMethod]
        public void FindsUnions()
        {
            var test = @"
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
    }

    public class Foo
    {
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
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = String.Format("Do not use Union without equality comparer"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 12, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresUnionsWithEqualityComparer()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
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
    }

    public class Foo
    {
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
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void FindsDistincts()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void Distinct()
        {
            var r = new[]
                {
                    new Foo()
                }
                .Distinct();
        }
    }

    public class Foo
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = String.Format("Do not use Distinct without equality comparer"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 12, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresDistinctsWithEqualityComparer()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void DistinctWithEqualityComparer()
        {
            var r = new[]
                {
                    new Foo()
                }
                .Distinct(
                    new FakeEqualityComparer<Foo>());
        }
    }

    public class Foo
    {
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
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void FindsGroupBy()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void GroupBy()
        {
            var r = new[]
            {
                new Foo()
            }.GroupBy(x => x);
        }
    }

    public class Foo
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = "Do not use GroupBy without equality comparer",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 12, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresGroupByWithEqualityComparer()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void GroupBy()
        {
            var r = new[]
            {
                new Foo()
            }.GroupBy(x => x, new FakeEqualityComparer<Foo>());
        }
    }

    public class Foo
    {
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
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void FindsContains()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void Contains()
        {
            var r = new[]
            {
                new Foo()
            }.Contains(new Foo());
        }
    }

    public class Foo
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = "Do not use Contains without equality comparer",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 12, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void FindsContainsAsMethodGroup()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void ContainsAsMethodGroup()
        {
            IEnumerable<string> a = null;
            IEnumerable<string> b = null;

            a.All(b.Contains);
        }
    }

    public class Foo
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = "Do not use Contains without equality comparer",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 15, 19)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresContainsWithEqualityComparer()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void ContainsWithEqualityComparer()
        {
            var r = new[]
            {
                new Foo()
            }.Contains(new Foo(), new FakeEqualityComparer<Foo>());
        }
    }

    public class Foo
    {
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
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void IgnoresContainsOnString()
        {
            var test = @"
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samples
{
    public class Sample
    {
        public void ContainsOnString()
        {
            string.Empty.Contains(string.Empty);
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        // won't run :-(
        //        [TestMethod]
        //        public void IgnoresContainsOnHashSet()
        //        {
        //            var test = @"
        //using System;
        //using System.Collections.Generic;
        //using System.Linq;

        //namespace Samples
        //{
        //    public class Sample
        //    {
        //        public void ContainsOnHashSet()
        //        {
        //            var hs = new HashSet<string>();

        //            hs.Contains(string.Empty);
        //        }
        //    }
        //}";

        //            VerifyCSharpDiagnostic(test);
        //        }

        [TestMethod]
        public void FindsEqualsOnObjects()
        {
            var test = @"
using System;

namespace Samples
{
    public class Sample
    {
        public void EqualsWithoutEquatable()
        {
            var a = new Foo();
            var b = new Foo();

            var r = a.Equals(b);
        }
    }

    public class Foo
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = "Do not use Equals on types not implementing IEquatable",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void FindsEqualsOnValueTypes()
        {
            var test = @"
using System;

namespace Samples
{
    public class Sample
    {
        public void EqualsWithoutEquatable()
        {
            var a = new Foo();
            var b = new Foo();

            var r = a.Equals(b);
        }
    }

    public struct Foo
    {
    }
}";

            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = "Do not use Equals on types not implementing IEquatable",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresEqualsWithEquatableType()
        {
            var test = @"
using System;

namespace Samples
{
    public class Sample
    {
        public void EqualsWithEquatable()
        {
            var a = new Equatable();
            var b = new Equatable();

            var r = a.Equals(b);
        }
    }

    public class Equatable : IEquatable<Equatable>
    {
        public bool Equals(Equatable other)
        {
            return true;
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void FindsEqualsWithoutEquatableTypeInGenericClass()
        {
            var test = @"
using System;

namespace Samples
{
    public class Sample<T>
    {
        public void Do(T t)
        {
            var r = t.Equals(null);
        }
    }
}";
            var expected = new DiagnosticResult
            {
                Id = "PreventEqualsMisusage",
                Message = "Do not use Equals on types not implementing IEquatable",
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 10, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void IgnoresEqualsWithEquatableTypeInGenericClass()
        {
            var test = @"
using System;

namespace Samples
{
    public class Sample<T>
        where T: IEquatable<T>
    {
        public void Do(T t)
        {
            var r = t.Equals(default(T));
        }
    }
}";

            VerifyCSharpDiagnostic(test);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new PreventEqualsMisusageAnalyzer();
        }
    }
}
