using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Tuple<T1>
{
    public T1 item1;
    public Tuple(T1 a)
    {
        item1 = a;
    }
}

public class Tuple<T1, T2>
{
    public T1 item1;
    public T2 item2;
    public Tuple(T1 a, T2 b)
    {
        item1 = a;
        item2 = b;
    }
}

public class Tuple<T1, T2, T3>
{
    public T1 item1;
    public T2 item2;
    public T3 item3;
    public Tuple(T1 a, T2 b, T3 c)
    {
        item1 = a;
        item2 = b;
        item3 = c;
    }
}

public class Tuple<T1, T2, T3, T4>
{
    public T1 item1;
    public T2 item2;
    public T3 item3;
    public T4 item4;
    public Tuple(T1 a, T2 b, T3 c, T4 d)
    {
        item1 = a;
        item2 = b;
        item3 = c;
        item4 = d;
    }
}
public class Tuple<T1, T2, T3, T4,T5>
{
    public T1 item1;
    public T2 item2;
    public T3 item3;
    public T4 item4;
    public T5 item5;
    public Tuple(T1 a, T2 b, T3 c, T4 d,T5 e)
    {
        item1 = a;
        item2 = b;
        item3 = c;
        item4 = d;
        item5 = e;
    }
}
public class Tuple<T1, T2, T3, T4, T5,T6>
{
    public T1 item1;
    public T2 item2;
    public T3 item3;
    public T4 item4;
    public T5 item5;
    public T6 item6;
    public Tuple(T1 a, T2 b, T3 c, T4 d, T5 e,T6 f)
    {
        item1 = a;
        item2 = b;
        item3 = c;
        item4 = d;
        item5 = e;
        item6 = f;
    }
}

