"use strict";

const API = "/api/todos";

const taskList = document.getElementById("taskList");
const taskForm = document.getElementById("taskForm");
const taskTitle = document.getElementById("taskTitle");
const taskCount = document.getElementById("taskCount");
const emptyState = document.getElementById("emptyState");
const emptyText = document.getElementById("emptyText");
const formError = document.getElementById("formError");
const clearCompletedBtn = document.getElementById("clearCompleted");
const progressFill = document.getElementById("progressFill");
const progressLabel = document.getElementById("progressLabel");

const filters = Array.from(document.querySelectorAll(".filter"));
let activeFilter = "all";
let tasks = [];

function formatDate(iso) {
  if (!iso) return "";
  try {
    const d = new Date(iso);
    return d.toLocaleDateString(undefined, { month: "short", day: "numeric" });
  } catch (e) {
    return "";
  }
}

async function api(method, path, body) {
  const options = { method };
  if (body !== undefined) {
    options.headers = { "Content-Type": "application/json" };
    options.body = JSON.stringify(body);
  }

  const res = await fetch(path, options);
  if (!res.ok) {
    let message = "HTTP " + res.status;
    try {
      const data = await res.json();
      if (data && data.error) message = data.error;
    } catch (e) { /* non-JSON error body */ }
    throw new Error(message);
  }
  return res.json();
}

function visibleTasks() {
  if (activeFilter === "active") return tasks.filter((t) => !t.completed);
  if (activeFilter === "completed") return tasks.filter((t) => t.completed);
  return tasks;
}

function renderTasks() {
  const list = visibleTasks();
  taskList.innerHTML = "";

  list.forEach((t) => {
    const li = document.createElement("li");
    li.className = "task" + (t.completed ? " is-done" : "");

    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.className = "checkbox";
    checkbox.checked = t.completed;
    checkbox.addEventListener("change", () => toggleTask(t.id, checkbox.checked));

    const title = document.createElement("span");
    title.className = "task-title";
    title.textContent = t.title;
    title.title = "Click to rename";
    title.addEventListener("click", () => beginEdit(t.id, title));

    const date = document.createElement("span");
    date.className = "task-date";
    date.textContent = formatDate(t.createdAt);

    const del = document.createElement("button");
    del.className = "task-delete";
    del.title = "Delete task";
    del.innerHTML = "\u00d7";
    del.addEventListener("click", () => deleteTask(t.id));

    li.append(checkbox, title, date, del);
    taskList.appendChild(li);
  });

  const remaining = tasks.filter((t) => !t.completed).length;
  taskCount.textContent = remaining + (remaining === 1 ? " task left" : " tasks left");

  const total = tasks.length;
  const done = total - remaining;
  const percent = total === 0 ? 0 : Math.round((done / total) * 100);
  progressFill.style.width = percent + "%";
  progressLabel.textContent = percent + "%";

  const isEmpty = list.length === 0;
  emptyState.hidden = !isEmpty;
  if (isEmpty) {
    emptyText.textContent =
      activeFilter === "all" ? "No tasks yet. Add your first one above."
      : activeFilter === "active" ? "Nothing left to do. Nice work!"
      : "No completed tasks yet.";
  }
}

async function loadTasks() {
  tasks = await api("GET", API);
  renderTasks();
}

async function addTask(event) {
  event.preventDefault();
  formError.hidden = true;

  const title = taskTitle.value.trim();
  if (!title) {
    formError.textContent = "Please enter a task title.";
    formError.hidden = false;
    return;
  }

  try {
    await api("POST", API, { title });
    taskTitle.value = "";
    await loadTasks();
    taskTitle.focus();
  } catch (e) {
    formError.textContent = e.message;
    formError.hidden = false;
  }
}

async function toggleTask(id, completed) {
  try {
    await api("PUT", API + "/" + id, { completed });
    await loadTasks();
  } catch (e) {
    alert(e.message);
  }
}

async function beginEdit(id, element) {
  const oldValue = element.textContent;
  const input = document.createElement("input");
  input.type = "text";
  input.className = "composer-input edit-input";
  input.value = oldValue;
  input.maxLength = 120;

  const commit = async () => {
    const newValue = input.value.trim();
    if (newValue && newValue !== oldValue) {
      try {
        await api("PUT", API + "/" + id, { title: newValue });
      } catch (e) {
        alert(e.message);
      }
    }
    await loadTasks();
  };

  input.addEventListener("keydown", (e) => {
    if (e.key === "Enter") { input.blur(); }
    if (e.key === "Escape") { input.value = oldValue; input.blur(); }
  });
  input.addEventListener("blur", commit);

  element.replaceWith(input);
  input.focus();
  input.select();
}

async function deleteTask(id) {
  if (!confirm("Delete this task?")) return;
  try {
    await api("DELETE", API + "/" + id);
    await loadTasks();
  } catch (e) {
    alert(e.message);
  }
}

async function clearCompleted() {
  const done = tasks.filter((t) => t.completed);
  if (!done.length) return;
  if (!confirm("Delete " + done.length + " completed task" + (done.length > 1 ? "s" : "") + "?")) return;

  try {
    await Promise.all(done.map((t) => api("DELETE", API + "/" + t.id)));
    await loadTasks();
  } catch (e) {
    alert(e.message);
  }
}

filters.forEach((btn) => {
  btn.addEventListener("click", () => {
    filters.forEach((b) => b.classList.remove("is-active"));
    btn.classList.add("is-active");
    activeFilter = btn.dataset.filter;
    renderTasks();
  });
});

clearCompletedBtn.addEventListener("click", clearCompleted);
taskForm.addEventListener("submit", addTask);
taskTitle.focus();

loadTasks().catch((e) => {
  formError.textContent = "Failed to load tasks: " + e.message;
  formError.hidden = false;
});
