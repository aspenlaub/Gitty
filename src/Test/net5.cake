#addin nuget:?package=PakledCore&loaddependencies=true&version=2.0.611.782

using Aspenlaub.Net.GitHub.CSharp.PakledCore;

var target = Argument("target", "Default");

Task("Default").Does(() => {
  Information("Constructing a strong thing");
  var strongThing = new StrongThing();
  if (strongThing.IsStrong) {
    Information("Success");
  } else {
    throw new Exception("Error");
  }
});

RunTarget(target);