
$(startup);


function startup()
{
	setInterval(playerPoll, 1000);

	g_cont = $('<div>').attr('class', 'container').appendTo($('body'));

	navigate('home');
}

var g_page;
var g_playerState;
var g_playerMinimized = true;
var g_playing;
var g_cont;
var g_stack = [];

function renderScreen()
{
	g_cont.empty();

	switch(g_page)
	{
		case 'home':
			renderMenu(
			[ 
				{ Title : 'Movies', handler: handleMovies },
				{ Title : 'TV', handler: handleTV },
				{ Title : 'Internet', handler: handleInternet },
				{ Title : 'Refresh', handler: handleRefresh }
			]);
			showBack(false);
			break;
		case 'movies':
			renderMovies();
			showBack(true);
			break;
		case 'tv':
			renderTV();
			showBack(true);
			break;
	}
}

function navigate(page)
{
	if (g_page)
	{
		g_stack.push(g_page);
	}

	g_page = page;
	renderScreen();
}

function navigateBack()
{
	g_page = g_stack.pop();

	renderScreen();
}

function showBack(show)
{
	var back = $('.backButton');
	if (back.length == 0)
	{
		back = $('<div>').attr('class', 'backButton button').text('Back').click(navigateBack).appendTo('body');
	}

	if (show)
	{
		back.show();
	}
	else
	{
		back.hide();
	}
}

function api(method, params, cb)
{
	$.ajax({
		url : '/api',
		type : 'POST',
		dataType : 'json',
		data : { method : method, params : params },
		success : function(data, status, xhr)
		{
			if (cb)
				cb(data);
		},
		error : function(xhr, status, error)
		{
			if (cb)
				cb(null, error);
		}
	});
}

function handleMovies()
{
	navigate('movies');
}

function renderMovies()
{
	var cont = g_cont;

	cont.empty();

	showBack(true);

	api('queryMovies', { sort : 'RecentlyAdded' }, function(movies, err)
	{
		for(var i = 0; i < movies.length; i++)
		{
			var movie = movies[i];

			var m = $('<div>').attr('class', 'movie').data('media', movie).click(handleMovieClick);

			$('<div>').text(movie.Title).attr('class', 'title').appendTo(m);

			cont.append(m);
		}
	});
}

function handleTV()
{
	navigate('tv');
}

function renderTV()
{
	var cont = g_cont;

	cont.empty();

	showBack(true);

	api('queryTV', { sort : 'RecentlyAdded' }, function(shows, err)
	{
		for(var i = 0; i <  shows.length; i++)
		{
			var show = shows[i];

			var m = $('<div>').attr('class', 'movie').data('media', show).click(handleMovieClick);

			$('<div>').text(show.Series + " S" + show.Season + "E" + show.Episode).attr('class', 'title').appendTo(m);

			cont.append(m);
		}
	});
}

function handleMovieClick(e)
{
	item = $(this).data('media');

	api('playerStart', { id : item.Id });

	g_playerMinimized = false;
}

function handleInternet()
{
}

function handleRefresh()
{
	api('refresh');
}

function renderMenu(items)
{
	var cont = g_cont;

	for(var i = 0; i < items.length; i++)
	{
		$('<div>').text(items[i].Title).attr('class', 'menuItem').appendTo(cont).click(items[i].handler);
	}
}

function updatePlayer()
{
	var cont = $('body');

	var min = $('.playerMinimized');
	if (min.length == 0)
	{
		$('<div>').attr('class', 'playerMinimized button').appendTo(cont)
			.append($('<div>').attr('class', 'title'))
			.append($('<div>').attr('class', 'progress'))
			.click(handleMaximize);

		min = $('.playerMinimized');
	}


	var max = $('.playerMaximized');
	if (max.length == 0)
	{
		$('<div>').attr('class', 'playerMaximized').appendTo(cont)
			.append($('<table>')
				.append($('<tr>')
					.append($('<td>').attr('colspan', '2')
						.append($('<div>').attr('class', 'title'))
					)
				)
				.append($('<tr>')
					.append($('<td>').attr('colspan', '2')
						.append($('<div>').attr('class', 'pauseplay button play').data('cmd', 'Pause').text('Play').click(handlePlayerButton))
					)
				)
				.append($('<tr>')
					.append($('<td>')
						.append($('<div>').attr('class', 'seekb1 button').text('<').data('cmd', 'SeekB1').click(handlePlayerButton))
					)
					.append($('<td>')
						.append($('<div>').attr('class', 'seekf1 button').text('>').data('cmd', 'SeekF1').click(handlePlayerButton))
					)
				)
				.append($('<tr>')
					.append($('<td>')
						.append($('<div>').attr('class', 'seekb2 button').text('<<').data('cmd', 'SeekB2').click(handlePlayerButton))
					)
					.append($('<td>')
						.append($('<div>').attr('class', 'seekf2 button').text('>>').data('cmd', 'SeekF2').click(handlePlayerButton))
					)
				)
				.append($('<tr>')
					.append($('<td>')
						.append($('<div>').attr('class', 'seekb3 button').text('<<<').data('cmd', 'SeekB3').click(handlePlayerButton))
					)
					.append($('<td>')
						.append($('<div>').attr('class', 'seekf3 button').text('>>>').data('cmd', 'SeekF3').click(handlePlayerButton))
					)
				)
				.append($('<tr>')
					.append($('<td>')
						.append($('<div>').attr('class', 'mute button').text('Mute').data('cmd', 'Mute').click(handlePlayerButton))
					)
					.append($('<td>')
						.append($('<div>').attr('class', 'audio button').text('Audio').data('cmd', 'CycleAudio').click(handlePlayerButton))
					)
				)
				.append($('<tr>')
					.append($('<td>')
						.append($('<div>').attr('class', 'back button').text('Back').click(handlePlayerBack))
					)
					.append($('<td>')
						.append($('<div>').attr('class', 'back button').text('Kill').click(handlePlayerKill))
					)
				)
			);


		max = $('.playerMaximized');
	}

	if (g_playerMinimized)
	{
		max.hide();
		min.find('.title').text(g_playerState == 'Idle' ? 'Idle' : g_playing.Title);
		min.show();
	}
	else
	{
		min.hide();
		max.find('.title').text(g_playerState == 'Idle' ? 'Idle' : g_playing.Title);
		max.find('.pauseplay').text(g_playerState == 'Playing' ? 'Pause' : 'Play');
		max.show();
	}
}

function handleMaximize()
{
	g_playerMinimized = false;
	updatePlayer();
}

function handlePlayerBack()
{
	g_playerMinimized = true;
	updatePlayer();
}

function handlePlayerKill()
{
	api('playerKill');

	g_playerMinimized = true;
	updatePlayer();
}

function handlePlayerButton()
{
	var cmd = $(this).data('cmd');

	api('playerCommand', { command : cmd });
}

function playerPoll()
{
	api('playerStatus', null, function(status, err)
	{
		if (!status)
			return;

		g_playerState = status.state;
		g_playing = status.playing;

		// If we go idle, make sure the player is minimized
		if (g_playerState == "Idle")
			g_playerMinimized = true;

		updatePlayer();
	});
}
