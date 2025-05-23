
using Application.Template;
using AutoMapper;
using Domain.DTO.Response;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class EmailHandler
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IEmailRepository _emailService;

        public EmailHandler(IOrderRepository orderRepository, IMapper mapper, IEmailRepository emailService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task InvoiceForEmail (int orderId)
        {
            var data = await _orderRepository.GetOrderItemsWithOrderIdAsync(orderId);
            if (data == null)
                throw new Exception("No Order Found");

            var invoiceDto = _mapper.Map<InvoiceForEmailDTO>(data);

            string html = EmailTemplateBuilder.BuildInvoiceHtml(invoiceDto);
            await _emailService.SendInvoiceEmailAsync(invoiceDto.Email, $"Hóa đơn đơn hàng #{invoiceDto.OrderId}", html);
        }
    }
}
