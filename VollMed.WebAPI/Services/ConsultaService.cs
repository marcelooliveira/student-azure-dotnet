﻿using VollMed.Web.Dtos;
using VollMed.Web.Exceptions;
using VollMed.Web.Interfaces;
using VollMed.Web.Models;

namespace VollMed.Web.Services
{
    public class ConsultaService : IConsultaService
    {
        private readonly IConsultaRepository _consultaRepository;
        private readonly IMedicoRepository _medicoRepository;
        private const int PageSize = 5;

        public ConsultaService(IConsultaRepository consultaRepository, IMedicoRepository medicoRepository)
        {
            _consultaRepository = consultaRepository;
            _medicoRepository = medicoRepository;
        }

        public async Task<PaginatedList<ConsultaDto>> ListarAsync(int? page)
        {
            var consultas = _consultaRepository.GetAllOrderedByData();
            IQueryable<ConsultaDto> dtos = consultas.Select(m => new ConsultaDto(m));
            return await PaginatedList<ConsultaDto>.CreateAsync(dtos, page ?? 1, PageSize);
        }

        public async Task CadastrarAsync(ConsultaDto dados)
        {
            var medicoConsulta = await _medicoRepository.FindByIdAsync(dados.IdMedico);
            if (medicoConsulta == null)
            {
                throw new RegraDeNegocioException("Medico não encontrado.");
            }

            if (dados.Id == 0)
            {
                var consulta = new Consulta(medicoConsulta, dados);
                await _consultaRepository.SaveAsync(consulta);
            }
            else
            {
                var consulta = await _consultaRepository.FindByIdAsync(dados.Id);
                if (consulta == null) throw new RegraDeNegocioException("Consulta não encontrada.");

                consulta.ModificarDados(medicoConsulta, dados);
                await _consultaRepository.UpdateAsync(consulta);
            }
        }

        public async Task<ConsultaDto> CarregarPorIdAsync(long id)
        {
            var consulta = await _consultaRepository.FindByIdAsync(id);
            if (consulta == null) throw new RegraDeNegocioException("Consulta não encontrada.");

            var medicoConsulta = await _medicoRepository.FindByIdAsync(consulta.Medico.Id);
            return new ConsultaDto(consulta.Id, consulta.Medico.Id, consulta.Paciente, consulta.Data, medicoConsulta!.Especialidade);
        }

        public async Task ExcluirAsync(long id)
        {
            await _consultaRepository.DeleteByIdAsync(id);
        }
    }
}


