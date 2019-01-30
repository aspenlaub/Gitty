var target = Argument("target", "Default");

Task("Default")
.Does(() => {
	Error("This is not a cake!");
});

RunTarget(target);