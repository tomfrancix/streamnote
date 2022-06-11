﻿/**
 * This file contains all the core code for the organizer
 */

/**
 * Open the modal for creating a new project.
 */
$('#createProjectModal').on('shown.bs.modal', function (e) {
    $.post({
        url: "/Projects/New",
        dataType: "html"
    })
        .done(function (result, status) {
            $("#createProjectBody").html("");
            $("#createProjectBody").append(result);
        });
});

/**
 * Open the modal for creating a new task.
 */
$('#createTaskModal').on('shown.bs.modal', function (e) {
    var projectId = $("#projectId").val();
    $.post({
        url: "/Task/New",
        data: {
            projectId: projectId
        },
        dataType: "html"
    })
        .done(function (result, status) {
            $("#createTaskBody").html("");
            $("#createTaskBody").append(result);
        });
});

/**
 * Save a newly created task.
 * @param {any} projectId
 */
function saveTask(projectId) {
    var taskTitle = $("#taskTitle").val();
    var taskDescription = $("#taskDescription").val();
    var taskIsPublic = $("#taskIsPublic").val();

    $.post({
        url: "/Task/Create",
        data: {
            projectId: projectId,
            title: taskTitle,
            description: taskDescription,
            isPublic: taskIsPublic
        },
        dataType: "html"
    })
    .done(function (result, status) {
        $('#createTaskModal').modal('hide');
        $("#newTasks").append(result);
    });
}

/**
 * Create a new step within a task.
 * @param {int} taskId The id of the task.
 */
function createStepModal(taskId) {
    $.post({
        url: "/Step/New",
        data: {
            taskId: taskId
        },
        dataType: "html"
    })
    .done(function (result, status) {
        $("#createStepBody").html("");
        $("#createStepBody").append(result);
        $('#createStepModal').modal('show')
    });
}

/**
 * Open the modal for creating a comment within a task.
 * @param {int} taskId The id of the task
 */
function createCommentModal(taskId) {
    $.post({
        url: "/TaskComment/New",
        data: {
            taskId: taskId
        },
        dataType: "html"
    })
        .done(function (result, status) {
            $("#createTaskCommentBody").html("");
            $("#createTaskCommentBody").append(result);
            $('#createTaskCommentModal').modal('show')
        });
}

/**
 * Save a newly created step.
 * @param {int} taskId The id of the task.
 */
function saveStep(taskId) {
    var stepContent = $("#stepContent").val();
    var stepDescription = $("#stepDescription").val();
    var stepIsPublic = $("#stepIsPublic").val();
    var stepsIdentifier = "#steps" + taskId;

    $.post({
        url: "/Step/Create",
        data: {
            taskId: taskId,
            content: stepContent,
            description: stepDescription,
            isPublic: stepIsPublic
        },
        dataType: "html"
    })
    .done(function (result, status) {
        $('#createStepModal').modal('hide');
        $(stepsIdentifier).append(result);
    });
}

/**
 * Save a newly created comment within a task.
 * @param {any} taskId
 */
function saveTaskComment(taskId) {
    var taskCommentContent = $("#taskCommentContent").val();

    var commentsIdentifier = "#comments" + taskId;
    $.post({
        url: "/TaskComment/Create",
        data: {
            taskId: taskId,
            content: taskCommentContent
        },
        dataType: "html"
    })
        .done(function (result, status) {
            $('#createTaskCommentModal').modal('hide');
            $(commentsIdentifier).append(result);
        });
}

/**
 * Expand a task when clicked on.
 * @param {int} taskId
 * @param {string} titleIdentifier
 * @param {string} editTitleIdentifier
 */
function expandTask(taskId, titleIdentifier, editTitleIdentifier) {
    var id = "#task" + taskId;
    var taskTab = "#taskTab" + taskId;
    titleIdentifier = "#" + titleIdentifier;
    editTitleIdentifier = "#" + editTitleIdentifier;

    if ($(id).css('display') == 'none') {
        $(id).show();
        $(titleIdentifier).hide();
        $(editTitleIdentifier).show();
        $(taskTab).attr("style", "color:#62ffd8");
    } else {
        $(id).hide();
        $(titleIdentifier).show();
        $(editTitleIdentifier).hide();
        $(taskTab).attr("style", "color:rgba(0,0,0,0.1)");
    }
}

/**
 * Change the status of a task.
 * @param {int} taskId
 * @param {int} taskStatus
 */
function changeTaskStatus(taskId, taskStatus) {
    var id = "#taskBox" + taskId;
    var task = $(id);
    var $element;
    $.post({
        url: "/Task/ChangeStatus",
        data: {
            taskId: taskId,
            status: taskStatus
        },
        dataType: "html"
    })
        .done(function (result, status) {
            task.remove();
            if (taskStatus == 0) {
                new Audio('/sounds/drop.wav').play();
                $element = $("#newTasks");
                $element.append(result);

            } else if (taskStatus == 1) {
                new Audio('/sounds/changeStatus.wav').play();
                $element = $("#yourTasks");
                $element.prepend(result);

            } else if (taskStatus == 2) {
                new Audio('/sounds/changeStatus.wav').play();
                $element = $("#completedTasks");
                $element.prepend(result);

            } else if (taskStatus == 3) {
                new Audio('/sounds/changeStatus.wav').play();
                $element = $("#completedTasks");
                $element.prepend(result);

            } else if (taskStatus == 4) {
                new Audio('/sounds/accepted.mp3').play();
            }

            $element.first("li").animate({
                backgroundColor: "green"
            }, 1000).delay(2000).queue(function () {
                $(this).animate({
                    backgroundColor: "red"
                }, 1000).dequeue();
            });
        });
}

/**
 * Change the status of a step within a task.
 * @param {int} stepId
 * @param {bool} isCompleted
 */
function changeStepStatus(stepId, isCompleted) {
    var id = "#stepIdentifier" + stepId;
    var step = $(id);
    $.post({
        url: "/Step/ChangeStatus",
        data: {
            stepId: stepId
        },
        dataType: "html"
    })
    .done(function (result, status) {

        // This 'isCompleted' came before we updated it!
        if (isCompleted == "False") {
            new Audio('/sounds/ding.mp3').play();
        }
        step.replaceWith(result);
    });
}

/**
 * Open the modal for editing a task.
 * @param {int} taskId The id of the task.
 */
function editTask(taskId) {
    $('#editTaskModal').modal('show');
}

/**
 * Edit the description of a task.
 * @param {any} el The submit description button element.
 * @param {any} taskId The id of the task.
 * @param {any} descriptionId The id of the description textarea on the view.
 */
function editTaskDescription(el, taskId, descriptionId) {
    var newDescription = $(descriptionId).val();

    $.post({
        url: "/Task/ChangeDescription",
        data: {
            taskId: taskId,
            description: newDescription
        },
        dataType: "html"
    })
    .done(function (result, status) {

        $(el).attr("style", "color:chartreuse;margin-top: -35px;").html("<i class='fa fa-check' />");

        setTimeout(function () {
            $(el).attr("style", "color:black;margin-top: -35px;").html("save");
        }, 2000);

        if (status == "success") {
            new Audio('/sounds/ding.mp3').play();
        }
    });
}

/**
 * This function allows the tasks to be sortable within their status containers.
 */
var adjustment;
$(function () {
    $("ol.todoSort").sortable({
        group: 'simple_with_animation',
        pullPlaceholder: false,
        // animation on drop
        onDrop: function ($item, container, _super) {
            var $clonedItem = $('<li/>').css({ height: 0 });
            $item.before($clonedItem);
            $clonedItem.animate({ 'height': $item.height() });

            $item.animate($clonedItem.position(), function () {
                $clonedItem.detach();
                _super($item, container);
            });

            var sql = ""
            $("ol.todoSort").children('li').each(function (index, element) {
                var taskId = $(this).attr('iden');
                if (taskId != undefined) {
                    sql += "UPDATE Tasks SET RANK = '" + (index + 1) + "' WHERE Id =" + $(this).attr('iden') + ";";
                }
            });

            $.post({
                url: "/Task/UpdateTaskOrder",
                data: {
                    query: sql
                },
                dataType: "html"
            })
                .done(function (result, status) {

                });
        },

        // set $item relative to cursor position
        onDragStart: function ($item, container, _super) {
            var offset = $item.offset(),
                pointer = container.rootGroup.pointer;

            adjustment = {
                left: pointer.left - offset.left,
                top: pointer.top - offset.top
            };

            _super($item, container);
        },
        onDrag: function ($item, position) {
            $item.css({
                left: position.left - adjustment.left,
                top: position.top - adjustment.top
            });
        },
        update: function (event, ui) {

        }
    });
});

/**
 * When the user presses the enter key while editing a task title, the new title is saved.
 */
$('.taskInput').bind('keyup',
    function (e) {

        if (e.keyCode === 13) { // 13 is enter key

            // Execute code here.
            var taskId = $(this).data("identifier");
            var newTitleValue = $(this).val();

            var data = {
                title: newTitleValue,
                taskId: taskId,
                dataType: "html"
            }

            $.post({
                    url: "/Task/UpdateTitle",
                    data: data
                })
                .done(function (result, status) {

                    var titleIdentifier = "#titleIdentifier" + taskId;

                    var $textDisplays = $(titleIdentifier).find(".taskTitleTextContainer");

                    for (var i = 0; i < $textDisplays.length; i++) {
                        var el = $textDisplays[i];

                        $(el).text(newTitleValue);
                    }

                    if (status == "success") {
                        new Audio('/sounds/ding.mp3').play();
                    }
                });
        }
    });