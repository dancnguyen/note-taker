window.noteTaker = {
  saveOnPageHide: function (notesService) {
    var flush = function () {
      try
      {
        notesService.invokeMethod("FlushPendingSave");
      }
      catch (e) { }
    };
    window.addEventListener("pagehide", flush);
    document.addEventListener("visibilitychange", function ()
    {
      if (document.visibilityState === "hidden")
      {
        flush();
      }
    });
  }
};
