const API_URL = 'http://localhost:5000/api/document';

function loadDocuments() {
  const searchTerm = $('#searchInput').val() ? $('#searchInput').val().trim() : "";
  const { start: startDate, end: endDate } = treatDateInterval($('#dateRange').val());
  const sortColumn = $('#sortColumn').val();
  const sortDirection = $('#sortDirection').val();

  const params = new URLSearchParams();

  if (searchTerm) params.append('searchTerm', searchTerm);
  if (startDate) params.append('startDate', startDate);
  if (endDate) params.append('endDate', endDate);
  if (sortColumn) params.append('orderBy', sortColumn);
  if (sortDirection) params.append('order', sortDirection);

  const queryParam = params.toString()

  $.get(`${API_URL}${queryParam ? `?${queryParam}` : '' }`, function (resp) {
    const rows = resp.data.map(document => `
      <tr>
        <td>${document.title}</td>
        <td>${document.description}</td>
        <td class="d-none d-md-table-cell">${document.archiveName}</td>
        <td class="d-none d-md-table-cell">${formatDate(document.creationDate)}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary downloadDocumentBtn" data-id="${document.id}" title="Download">
            <i class="bi bi-download"></i>
          </button>

          <button class="btn btn-sm btn-warning editDocumentBtn" data-id="${document.id}" title="Editar">
            <i class="bi bi-pencil-fill"></i>
          </button>

          <button class="btn btn-sm btn-danger deleteDocumentBtn" data-id="${document.id}" title="Excluir">
            <i class="bi bi-trash-fill"></i>
          </button>
        </td>
      </tr>
    `);
    $('#documentTable').html(rows.join(''));
  });

  function formatDate(isoDate) {
    const [date, isoHour] = isoDate.split('T')
    const [year, mount, day] = date.split('-')
    hour = isoHour.split('.')[0]
    
    return `${day}/${mount}/${year} ${hour}`
  }
}

function treatDateInterval(dateIntervalValue) {
  const parts = dateIntervalValue.split(' até ');

  const parseToISO = (str) => {
    if (!str || str.trim() === "") return null;
    const [dia, mes, ano] = str.split('/');
    return `${ano}-${mes}-${dia}`;
  };

  return {
    start: parts[0] ? parseToISO(parts[0]) : null,
    end: parts[1] ? parseToISO(parts[1]) : null
  };
}

function openDocumentModal(doc = null) {
  const modalHtml = $('#documentModal');
  const fileInput = document.getElementById('fileInput');

  if (doc) {
    // Modo edição
    $('#documentModalLabel').text('Editar Documento');
    $('#documentId').val(doc.id);
    $('#title').val(doc.title);
    $('#description').val(doc.description);
    $('#uploadedFileName').text(doc.archiveName);
    $('#uploadedFileInfo').removeClass('d-none');

    // Buscar e simular o arquivo já enviado
    fetch(`${API_URL}/${doc.id}/download`)
      .then(response => {
        if (!response.ok) throw new Error('Erro ao buscar o arquivo');
        return response.blob();
      })
      .then(blob => {
        const file = new File([blob], doc.archiveName, { type: blob.type });
        const dataTransfer = new DataTransfer();
        dataTransfer.items.add(file);
        fileInput.files = dataTransfer.files;
      })
      .catch(error => {
        console.error('Erro ao simular upload do arquivo:', error);
        fileInput.value = '';
      });
  } else {
    // Modo novo
    $('#documentModalLabel').text('Novo Documento');
    $('#documentForm')[0].reset();
    $('#documentId').val('');
    $('#uploadedFileInfo').addClass('d-none');
    $('#uploadedFileName').text('');
    fileInput.value = '';
  }

  modalHtml.modal("show");
}


$('#btnAdd').on('click', function () {
  openDocumentModal();
});

$(document).on('click', '.downloadDocumentBtn', function () {
  const id = $(this).data('id');
  window.open(`${API_URL}/${id}/download`, '_blank');
});

$(document).on('click', '.editDocumentBtn', function () {
  const id = $(this).data('id');

  $.get(`${API_URL}/${id}`, function (resp) {
    openDocumentModal(resp.data);
  });
});

$(document).on('click', '.deleteDocumentBtn', function () {
  const id = $(this).data('id');

  $.ajax({
    url: `${API_URL}/${id}`,
    type: 'DELETE',
    success: function () {
      loadDocuments();
    },
    error: function () {
      alert('Erro ao excluir documento');
    }
  });
});

$(document).on('click', '.sortable', function () {
  let clickedColumn = $(this).data('column');
  const currentColumn = $('#sortColumn').val();
  const currentDirection = $('#sortDirection').val();

  let newDirection = 'asc';

  if (currentColumn === clickedColumn) {
    if (currentDirection === 'asc') newDirection = 'desc';
    else if (currentDirection === 'desc') {
      clickedColumn = '';
      newDirection = '';
    }
  }

  $('#sortColumn').val(clickedColumn);
  $('#sortDirection').val(newDirection);

  $('.sortable span').text('');
  if (newDirection === 'asc') $(this).find('span').text('↑');
  else if (newDirection === 'desc') $(this).find('span').text('↓');

  loadDocuments();
});

$('#searchInput').on('input', loadDocuments);
$('#dateRange').on('change', loadDocuments);

function initDocuments() {
  console.log('initDocuments chamado');

  flatpickr("#dateRange", {
    mode: "range",
    dateFormat: "d/m/Y",
    allowInput: false,
    locale: {
      firstDayOfWeek: 1,
      rangeSeparator: ' até ',
      weekdays: {
        shorthand: ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'],
        longhand: ['Domingo', 'Segunda-feira', 'Terça-feira', 'Quarta-feira', 'Quinta-feira', 'Sexta-feira', 'Sábado'],
      },
      months: {
        shorthand: ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez'],
        longhand: ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'],
      }
    }
  });

  loadDocuments();
}

$('#saveDocument').on('click', function() {
  const form = document.getElementById('documentForm');
  const formData = new FormData(form);

  const documentId = formData.get('documentId') || $('#documentId').val();

  formData.append('title', $('#title').val());
  formData.append('description', $('#description').val());

  const fileInput = document.getElementById('fileInput');
  if (fileInput.files.length > 0) {
    formData.append('archive', fileInput.files[0]);
  }

  const method = documentId ? 'PUT' : 'POST';
  const url = documentId ? `${API_URL}/${documentId}` : API_URL;

  fetch(url, {
    method: method,
    body: formData
  })
    .then(response => {
      if (!response.ok) throw new Error('Erro ao salvar o documento');
      return response.json();
    })
    .then(data => {
      $('#documentModal').modal('hide');
    })
    .catch(err => {
      console.error(err);
    });
})