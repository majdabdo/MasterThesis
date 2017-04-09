FROM mono:4.2.3.4

RUN mkdir -p container 
COPY Release/* /container/

CMD [ "mono", "container/DEBS17.exe" ]