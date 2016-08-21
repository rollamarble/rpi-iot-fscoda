// Compiler output follows.
namespace Fsc { 
using System; 
using System.Collections.Generic;
using YieldProlog;
public class FscContext {
public class YPInnerClass { }
public static Type getDeclaringClass() { return typeof(YPInnerClass).DeclaringType; }

public static IEnumerable<bool> rover_request(object IdChat, object Cmd)
{
    {
        foreach (bool l2 in YP.matchDynamic(Atom.a("rover_request_"), new object[] { IdChat, Cmd }))
        {
            yield return false;
        }
    }
}

public static IEnumerable<bool> rover_command(object Cmd, object Out)
{
    {
        foreach (bool l2 in YP.matchDynamic(Atom.a("rover_command_"), new object[] { Cmd, Out }))
        {
            yield return false;
        }
    }
}

public static IEnumerable<bool> rover_validate(object Out, object Status)
{
    {
        foreach (bool l2 in YP.matchDynamic(Atom.a("rover_validate_"), new object[] { Out, Status }))
        {
            yield return false;
        }
    }
}

}}