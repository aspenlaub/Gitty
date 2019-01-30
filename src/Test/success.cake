var target = Argument("target", "Default");

Task("Default")
.Does(() => {
	Information("That is what I call a cake!");
});

Task("AlternativeTarget")
  .Does(() => {
    Information("This is an alternative target");   
});

RunTarget(target);