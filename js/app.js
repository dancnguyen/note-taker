(function () {
  var blockTags = /^(P|DIV|H[1-6]|UL|OL|LI|BLOCKQUOTE|PRE|HR|TABLE|TR|BR)$/;

  function wrap(text, marker) {
    var match = /^(\s*)([\s\S]*?)(\s*)$/.exec(text);
    return match[2] ? match[1] + marker + match[2] + marker + match[3] : text;
  }

  function walkChildren(node, markdown) {
    var out = "";
    node.childNodes.forEach(function (child) { out += walk(child, markdown); });
    return out;
  }

  function walkList(node, markdown) {
    var lines = [];
    var index = 0;
    node.childNodes.forEach(function (child) {
      if (child.tagName !== "LI") return;
      index++;
      var marker = node.tagName === "OL" ? index + ". " : "- ";
      var body = walkChildren(child, markdown).replace(/\n{2,}/g, "\n").trim();
      lines.push(marker + body.split("\n").join("\n" + " ".repeat(marker.length)));
    });
    return "\n" + lines.join("\n") + "\n";
  }

  function walk(node, markdown) {
    if (node.nodeType === Node.TEXT_NODE) {
      var text = node.nodeValue.replace(/[ \t\r\n]+/g, " ").replace(/ /g, " ");
      var previous = node.previousSibling;
      if (!text.trim() && (!previous || blockTags.test(previous.nodeName))) return "";
      return text;
    }

    if (node.nodeType !== Node.ELEMENT_NODE) return "";

    var tag = node.tagName;
    if (tag === "UL" || tag === "OL") return walkList(node, markdown);
    if (tag === "BR") return "\n";
    if (tag === "HR") return markdown ? "\n\n---\n\n" : "\n\n";
    if (tag === "PRE") return markdown ? "\n\n```\n" + node.textContent + "\n```\n\n" : "\n\n" + node.textContent + "\n\n";
    if (tag === "IMG") return markdown ? "![" + (node.getAttribute("alt") || "") + "](" + (node.getAttribute("src") || "") + ")" : node.getAttribute("alt") || "";

    var inner = walkChildren(node, markdown);
    var heading = /^H([1-6])$/.exec(tag);
    if (heading) return "\n\n" + (markdown ? "#".repeat(+heading[1]) + " " : "") + inner.trim() + "\n\n";

    switch (tag) {
      case "P":
        return "\n\n" + inner + "\n\n";
      case "DIV":
      case "TR":
        return "\n" + inner + "\n";
      case "TD":
      case "TH":
        return inner + "\t";
      case "BLOCKQUOTE":
        return markdown ? "\n\n" + inner.trim().split("\n").map(function (line) { return "> " + line; }).join("\n") + "\n\n" : "\n\n" + inner + "\n\n";
      case "B":
      case "STRONG":
        return markdown ? wrap(inner, "**") : inner;
      case "I":
      case "EM":
        return markdown ? wrap(inner, "*") : inner;
      case "S":
      case "STRIKE":
      case "DEL":
        return markdown ? wrap(inner, "~~") : inner;
      case "CODE":
        return markdown ? wrap(inner, "`") : inner;
      case "A":
        var href = node.getAttribute("href");
        if (!href) return inner;
        if (markdown) return "[" + inner + "](" + href + ")";
        return inner.trim() === href ? inner : inner + " (" + href + ")";
      default:
        return inner;
    }
  }

  function toText(html, markdown) {
    var body = new DOMParser().parseFromString(html, "text/html").body;
    var text = walk(body, markdown)
      .replace(/[ \t]+\n/g, "\n")
      .replace(/\n{3,}/g, "\n\n")
      .trim();
    return text ? text + "\n" : "";
  }

  function escapeHtml(text) {
    var element = document.createElement("div");
    element.textContent = text;
    return element.innerHTML;
  }

  function toHtmlDocument(title, html) {
    return "<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\" />\n<title>" + escapeHtml(title) +
      "</title>\n</head>\n<body>\n" + html + "\n</body>\n</html>\n";
  }

  function convert(title, html, extension) {
    switch (extension) {
      case ".md": return toText(html, true);
      case ".html": return toHtmlDocument(title, html);
      default: return toText(html, false);
    }
  }

  function download(blob, fileName) {
    var url = URL.createObjectURL(blob);
    var link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
  }

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
    },

    exportNote: async function (title, html, format) {
      var fileName = title + format.extension;
      var blob = new Blob([convert(title, html || "", format.extension)], { type: format.mimeType + ";charset=utf-8" });

      if (window.showSaveFilePicker) {
        var handle;
        try {
          var accept = {};
          accept[format.mimeType] = [format.extension];
          handle = await window.showSaveFilePicker({
            suggestedName: fileName,
            types: [{ description: format.name, accept: accept }]
          });
        } catch (e) {
          if (e.name === "AbortError") return "cancelled";
          if (e.name !== "SecurityError") throw e;
        }

        if (handle) {
          var writable = await handle.createWritable();
          await writable.write(blob);
          await writable.close();
          return "saved";
        }
      }

      download(blob, fileName);
      return "downloaded";
    }
  };
})();
