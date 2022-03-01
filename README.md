[![Publish Docker image](https://github.com/AntoniosBarotsis/NoRussian/actions/workflows/publish.yml/badge.svg)](https://github.com/AntoniosBarotsis/NoRussian/actions/workflows/publish.yml)

# No Russian

This project leverages [TPL](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library)
to make continuous HTTP GET requests on a list of URLs for purely educational reasons that have nothing to do with the name
of the project and or any large scale worldwide events that may be happening at the time of writing this. Also comes in the form of a
[container](https://hub.docker.com/repository/docker/antoniosbarotsis/norussian).

By default, this will look for a file `./NoRussian/data.txt` but you can also supply the links as an environment variable
in the form of a semi colon delimited string that looks like this `link1;link2`.

This is dead simple with no exception handling anywhere and was just made for the fun of it and to explore some parallel programming
and data pipeline basics, don't take it seriously.

Some docker copy pastas.

```bash
docker build --no-cache -t norussian .
docker run -d --name norussian --env Throttle=0 norussian
docker logs norussian -f
```

Env variables:

- `RetryDelay`
- `RequestTimeout`
- `Threads`
- `Throttle`
- `Links`
