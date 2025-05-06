$(document).ready(function () {
  $("#navbar-container").load("./components/navbar/navbar.html");

  $("#sidebar-container").load("./components/sidebar/sidebar.html", function () {
    const lastVisitedPage = sessionStorage.getItem('lastVisitedPage');
    const selector = lastVisitedPage
      ? `.sidebar-link[data-page="${lastVisitedPage}"]`
      : '.sidebar-link[data-page="home/home"]';

    $(selector).trigger('click');
  });

  $(document).on('click', '#menu-toggle', function () {
    $("#wrapper").toggleClass("toggled");
  });

  $(document).on('click', '.sidebar-link', function (e) {
    e.preventDefault();

    const page = $(this).data('page');
    const modal = $(this).data('modal');

    if (modal) {
      const modalUrl = `./components/${modal}/${modal}.html`;
      $.get(modalUrl, function (data) {
        $("body").append(data);
        const modalInstance = $(`#${modal}Modal`);
        modalInstance.modal('show');
      }).fail(function () {
        alert("Erro ao carregar modal.");
      });
    }

    if (page) {
      sessionStorage.setItem('lastVisitedPage', page);

      const url = `./pages/${page}.html`;
      const [folder, file] = page.split('/');
      const scriptUrl = `./pages/${folder}/${file}.js`;
      const cssUrl = `./pages/${folder}/${file}.css`;

      $("#content").load(url, function (response, status) {
        if (status === "error") {
          $("#content").html("<p>Erro ao carregar a página.</p>");
        } else {
          if (!$(`link[href="${cssUrl}"]`).length) {
            const link = document.createElement("link");
            link.rel = "stylesheet";
            link.href = cssUrl;
            document.head.appendChild(link);
          }

          const existingScript = document.querySelector(`script[src="${scriptUrl}"]`);

          if (!existingScript) {
            const script = document.createElement("script");
            script.src = scriptUrl;
            script.type = "text/javascript";
            script.onload = function () {
              if (typeof initDocuments === 'function') {
                initDocuments();
              }
            };
            document.body.appendChild(script);
          } else {
            // Já está carregado — executa direto se possível
            if (typeof initDocuments === 'function') {
              initDocuments();
            } else {
              // Em caso de carregamento assíncrono ou cache atrasado, espera o próximo tick
              setTimeout(() => {
                if (typeof initDocuments === 'function') {
                  initDocuments();
                }
              }, 100);
            }
          }
        }
      });
    }
  });
});
